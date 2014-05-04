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

public function set_name($name)
{
$this->name = $name;
}
}

$Johnny = new person('male');
echo $Johnny->get_gender();

$Mary = new person('female');
$Mary->set_name('Mary');
?>