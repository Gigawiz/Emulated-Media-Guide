<?php
//
// tvtv2xmltv Guide Data
// - This script will extract guide data from "tvtv.us" and produce an "XmlTV" data file
// - Set the options for the guide data you want to extract below
// - Host this on a php enabled web server
// - Configure your TV Guide software to use it as a data source (Jellyfin in my case)
//
// https://www.tvtv.us/
// http://wiki.xmltv.org/index.php/XMLTVFormat
//

//
// ### Main Site
// https://www.tvtv.us/
//
// ### Search By Zip Code
// https://www.tvtv.us/search/30236
//
// ### Channel lineups
// https://www.tvtv.us/api/ga/jonesboro/30236
//
// ### Lineup Data
// https://www.tvtv.us/tvm/t/tv/v4/lineups/15129
//
// ### League Data
// https://www.tvtv.us/tvm/t/tv/v4/leagues
//
// ### All Channels
// https://www.tvtv.us/tvm/t/tv/v4/lineups/15129/listings/grid?detail=%27brief%27&start=2020-12-22T06:00:00.000Z&end=2020-12-23T05:59:00.000Z&startchan=2-1&endchan=69-5
//
// ### Episode Details
// https://www.tvtv.us/tvm/t/tv/v4/episodes/219394
//
// ### Channel Logo
// https://cdn.tvpassport.com/image/station/100x100/dabl.png
//
// ### Show Picture
// https://cdn.tvpassport.com/image/show/480x720/95000.jpg
//
//

date_default_timezone_set(timezone_name_from_abbr("EST"));
$tzOffset = "-0000";

$lineUpID = "15129";    // Set this to ID of the Line Up data you want to extract
$startChannel = "2-1";  // Set starting channel
$endChannel = "69-5";   // Send ending channel

$now = strtotime ( "now UTC" );
$end = strtotime ( "+12 hours", $now ); // Adjust this to get more or less guide data

$startTime = date ( 'Y-m-d\TH:00:00.000\Z', $now ); //"2020-12-22T06:00:00.000Z";
$endTime = date ( 'Y-m-d\TH:00:00.000\Z', $end ); //"2020-12-23T05:59:00.000Z";
$listingData = "https://www.tvtv.us/tvm/t/tv/v4/lineups/$lineUpID/listings/grid?detail=%27brief%27&start=$startTime&end=$endTime&startchan=$startChannel&endchan=$endChannel";

// GET guide data
$json = callAPI( 'GET', $listingData, false );
$data = json_decode( $json, true );

// Uncomment the following two lines and make the script directory writeable by the webserver to save the guide data
//$json = json_encode( $data, JSON_PRETTY_PRINT );
//file_put_contents("xmltv.json", $json);

$url = "http". ( !empty ( $_SERVER['HTTPS'] ) ? "s" : "" )."://".$_SERVER['HTTP_HOST'].$_SERVER['REQUEST_URI'];

echo "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n";
echo "<!DOCTYPE tv SYSTEM \"xmltv.dtd\">\r\n";
echo "<tv date=\"$startTime\" source-info-url=\"$url\" source-info-name=\"tvtv2xmltv\">";

foreach ( $data as &$channel )
{
    // Channel Data
    echo "<channel id=\"".$channel["channel"]["stationID"]."\">";
    echo "<display-name>".$channel["channel"]["name"]."</display-name>";
    echo "<display-name>".$channel["channel"]["callsign"]."</display-name>";
    echo "<display-name>".$channel["channel"]["number"]."</display-name>";
    echo "<url>".$channel["channel"]["webLink"]."</url>";
    echo "<icon src=\"https://cdn.tvpassport.com/image/station/100x100/".$channel["channel"]["logoFilename"]."\"/>";
    echo "</channel>\r\n";


    // Program Data
    foreach ( $channel["listings"] as &$listing )
    {
        $tStart = strtotime ( $listing['listDateTime'].' UTC' );
        $tEnd = strtotime ( "+".$listing['duration']." minutes", $tStart );
        $listingStart = date ( "YmdHis", $tStart );
        $listingEnd = date ( "YmdHis", $tEnd );

        $showName = htmlspecialchars ( $listing['showName'], ENT_XML1, 'UTF-8' );

        if ( $showName == "Movie" )
        {
            $showName = htmlspecialchars ( $listing['episodeTitle'], ENT_XML1, 'UTF-8' );
            $episodeTitle = "";
        }
        else
        {
            $episodeTitle = htmlspecialchars ( $listing['episodeTitle'], ENT_XML1, 'UTF-8' );
        }

        $showDescription = htmlspecialchars ( $listing['description'], ENT_XML1, 'UTF-8' );

        $categories = explode ( ", ", htmlspecialchars ( $listing['showType'], ENT_XML1, 'UTF-8' ) );
        $hosts = explode ( ", ", htmlspecialchars ( $listing['showHost'], ENT_XML1, 'UTF-8' ) );
        $guests = explode ( ", ", htmlspecialchars ( $listing['guest'], ENT_XML1, 'UTF-8' ) );

        echo "<programme start=\"".$listingStart." ".$tzOffset."\" stop=\"".$listingEnd." ".$tzOffset."\" duration=\"".$listing['duration']."\" channel=\"".$channel["channel"]["stationID"]."\">";
        echo "<title lang=\"en\">".$showName."</title>";
        echo "<sub-title lang=\"en\">".$episodeTitle."</sub-title>";
        echo "<desc lang=\"en\">".$showDescription."</desc>";

        echo "<episode-num system=\"onscreen\">".trim ( $listing['episodeNumber'] )."</episode-num>";
        echo "<icon src=\"https://cdn.tvpassport.com/image/show/480x720/".$listing["showPicture"]."\"/>";

        for ( $i = 0; $i < count ( $categories ); $i++ )
        {
            echo "<category lang=\"en\">".$categories[$i]."</category>";
        }

        echo "<credits>";

        for ( $i = 0; $i < count ( $hosts ); $i++ )
        {
            echo "<actor>".$hosts[$i]."</actor>";
        }

        for ( $i = 0; $i < count ( $guests ); $i++ )
        {
            echo "<guest>".$guests[$i]."</guest>";
        }

        echo "</credits>";

        echo "<rating>";
        echo "<value>".$listing["rating"]."</value>";
        echo "</rating>";
        echo "<star-rating>";
        echo "<value>".$listing["starRating"]." / 5</value>";
        echo "</star-rating>";
        echo "<video>";

        if ( $listing["blackWhite"] )
        {
            echo "<colour>No</colour>";
        }
        else
        {
            echo "<colour>Yes</colour>";
        }

        if ( $listing["hd"] )
        {
            echo "<quality>HDTV</quality>";
        }

        echo "</video>";

        if ( $listing["new"] )
        {
            echo "<new />";
        }

        if ( $listing["repeat"] )
        {
            echo "<previously-shown />";
        }

        if ( $listing["seriesPremiere"] || $listing["seasonPremiere"] )
        {
            echo "<premiere />";
        }

        echo "</programme>\r\n";
    }
}

echo "</tv>";


// https://weichie.com/blog/curl-api-calls-with-php/
function callAPI ( $method, $url, $data )
{
    $curl = curl_init();

    switch ( $method )
    {
        case "POST":
            curl_setopt ( $curl, CURLOPT_POST, 1 );

            if ( $data )
                curl_setopt ( $curl, CURLOPT_POSTFIELDS, $data );

            break;

        case "PUT":
            curl_setopt ( $curl, CURLOPT_CUSTOMREQUEST, "PUT" );

            if ( $data )
                curl_setopt ( $curl, CURLOPT_POSTFIELDS, $data );

            break;

        default:
            if ( $data )
                $url = sprintf ( "%s?%s", $url, http_build_query ( $data ) );
    }

    // OPTIONS:
    curl_setopt ( $curl, CURLOPT_URL, $url );
    curl_setopt ( $curl, CURLOPT_HTTPHEADER, array (
                      'Content-Type: application/json',
                      'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0',
                  ) );
    curl_setopt ( $curl, CURLOPT_RETURNTRANSFER, 1 );
    curl_setopt ( $curl, CURLOPT_HTTPAUTH, CURLAUTH_BASIC );

    // EXECUTE:
    $result = curl_exec ( $curl );

    if ( !$result )
    {
        die ( "Connection Failure" );
    }

    curl_close ( $curl );
    return $result;
}