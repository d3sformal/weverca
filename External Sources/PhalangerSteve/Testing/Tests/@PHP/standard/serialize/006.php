[expect php]
[file]
<?php
include('Phalanger.inc');
	$� = array('�' => '�');

	class � 
	{
		public $� = '�';
	}
  
    $foo = new �();
  
	__var_dump(serialize($foo));
	__var_dump(unserialize(serialize($foo)));
	__var_dump(serialize($�));
	__var_dump(unserialize(serialize($�)));
?>
