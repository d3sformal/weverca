[expect ct-error]
[file]
<?php

error_reporting(0);

class pass {
	private static function show() {
		echo "Call pass::show()\n";
	}

	public static function do_show() {
		pass::show();
	}
}

pass::do_show();

class fail {
	public static function show() {
		echo "Call fail::show()\n";
		pass::show();
	}
}

fail::show();

echo "Done\n"; // shouldn't be displayed
?>