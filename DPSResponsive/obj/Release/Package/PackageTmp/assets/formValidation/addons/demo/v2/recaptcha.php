<?php
// Register the public and private keys at https://www.google.com/recaptcha/admin
define('SITE_KEY',   '6LdgOwETAAAAALA9auuNVKFeXizXcYFrKOVC_vs-');
define('SECRET_KEY', '6LdgOwETAAAAAAHEd6l5XR5JOkBJDgUS4BPqxQrk');

// https://github.com/google/ReCAPTCHA/tree/master/php
require_once('recaptchalib.php');

$reCaptcha = new ReCaptcha(SECRET_KEY);

// Verify the captcha
// https://developers.google.com/recaptcha/docs/verify
$resp = $reCaptcha->verifyResponse($_SERVER['REMOTE_ADDR'], $_POST['g-recaptcha-response']);

echo json_encode(array(
    'valid'   => $resp->success,
    'message' => $resp->success ? null  : 'Hey, the captcha is wrong!',       // $resp->errorCodes,
));
