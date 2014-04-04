<?php
/**
 * Very simplistic version of PEAR.php
 */
class NoccException {
    /**
     * Message
     * @var string
     */
    private $message = '';

    /**
     * Initialize the exception
     * @param string $message Message
     */
    public function __construct($message = 'unknown error') {
        $this->message = $message;
    }

    /**
     * Get the message from the exception
     * @return string Message
     */
    public function getMessage() {
        return $this->message;
    }

    /**
     * ...
     * @param object $data Data
     * @return boolean Is exception?
     */
    public static function isException($data) {
        return (bool)(is_object($data) &&
                      ((get_class($data) == "NoccException") ||
               (get_class($data) == "noccexception")));
    }
}