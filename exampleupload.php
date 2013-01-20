<?php
/*
WoodShots Image Upload Script
Written by Tobias Mädel (t.maedel@alfeld.de)
http://tbspace.de
*/
$username = "nutzername";
$password = "passwort";
$imagefolder = "pics/";
$httpurl  = "http://tbspace.de/pics/";

function generateURL()
{
	$dummy	= array_merge(range('a', 'z'));
	mt_srand((double)microtime()*1000000);
	for ($i = 1; $i <= (count($dummy)*2); $i++)
	{
		$swap		= mt_rand(0,count($dummy)-1);
		$tmp		= $dummy[$swap];
		$dummy[$swap]	= $dummy[0];
		$dummy[0]	= $tmp;
	}
	return substr(implode('',$dummy),0,10);
}
$filename = generateURL().".png";
if ($_POST['username'] == $username)
{
	if ($_POST['password'] == $password)
	{
		echo "#success ".$httpurl.$filename;
		move_uploaded_file($_FILES['image']['tmp_name'], $imagefolder.$filename); 
	}
}
?>