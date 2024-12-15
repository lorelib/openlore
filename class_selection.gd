extends ItemList

func _on_item_selected(index:int):
	var cam = $"../CameraPosition"
	if cam == null:
		return
	match get_item_text(index):	
		"1": 
			cam.set_position( Vector3(600.0, 1475.0, -180.0))
		"2":
			cam.set_position( Vector3(603.5, -328.0, -85.0))
		"3":
			cam.set_position( Vector3(-847.0, -250.0, -180.0))
		"4":
			cam.set_position( Vector3(-45.0, 1415.0, 0.0))
		"5":
			cam.set_position( Vector3(0.0, 680.0, 135.0))
		"6":
			# sets some value to 2.2
			cam.set_position( Vector3(60.0, -950.0, 515.0))
		"7":
			# sets some day period to 0x17
			cam.set_position( Vector3(0.0, -1120.0, 385.0))
		"8":
			# sets some day period to 0x17
			cam.set_position( Vector3(60.0, -230.0, -870.0))
		"9":
			# sets some day period to 0x17
			cam.set_position( Vector3(-790.0, 640.0, -20.0))
		"10":
			cam.set_position( Vector3(2.0, 490.0, -981.0))
		"11":
			# sets some day period to 0x17
			cam.set_position( Vector3(-850.0, -1275.0, -330.0))
		"12":
			# sets some day period to 0x17
			cam.set_position( Vector3(600.0, 680.0, -25.0))
		"13":
			cam.set_position( Vector3(-840.0, -1274.0, -205.0))
		"14":
			cam.set_position( Vector3(-885.0, 685.0, 160.0))
		"15":
			cam.set_position( Vector3(0.0, -260.0, -720.0))
