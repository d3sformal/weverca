<?
class human {
public $gender;

public function __construct($gender)
{
$this->gender = $gender;

echo self::get_gender();
}

public function get_gender()
{
return $this->gender;
}
}

class person extends human {
public $name;
public $surname;

public function set_name($name)
{
$this->name = $name;
}

public function set_surname($surname)
{
$this->surname = $surname;
}

public  function get_name()
{
return $this->name;
}

public  function get_surname()
{
return $this->surname;
}
}

$Johnny = new person('male');
$Johnny->set_name('Johnny');
$Johnny->set_surname('Williams');

echo $Johnny->get_name().' '.$Johnny->get_surname().' is a '.$Johnny->get_gender();

$Mary = new person('female');
$Mary->set_name('Mary');
$Mary->set_surname('Williams');

echo $Mary->get_name().' '.$Mary->get_surname().' is a '.$Johnny->get_gender();
?>