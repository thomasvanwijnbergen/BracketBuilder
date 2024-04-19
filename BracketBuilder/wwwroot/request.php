<?php
// This file is ment to be uploaded on a webserver with a workoing mailserver (strato for example)
// UserAccount.EmailUrl should be pointing towards the request file, and then you can mail
// either that or find out some other way to mail

// $to = base64_decode($_GET['to']);

// $subject = base64_decode($_GET['subject']);

// $message = base64_decode($_GET['message']);

// $from = base64_decode($_GET['from']);

$to = $_GET['to'];

$subject = $_GET['subject'];

$message = $_GET['message'];

$from = $_GET['from'];

$headers = "From:" . strip_tags($from) . "\r\n";
$headers .= "MIME-Version: 1.0\r\n";
$headers .= "Content-Type: text/html; charset=UTF-8\r\n";

$mail = mail($to,$subject,$message,$headers);

if($mail)
{
	echo "Email sent to: ".$to;
	echo "</br>";
	echo "with subject: ".$subject;
	echo "</br>";
	echo "with content: ".$message;
	echo "</br>";
	echo "From: ".$from;
}

?>
