extends Button

func _on_button_pressed():
	var resources = get_tree().current_scene._resources
	var character = resources.InstantiateCharacter("elm")
	character.AttachItem("it149", "r_point", resources)
	character.AttachItem("it227", "shield_point", resources)
	
	
	# get_tree().current_scene.Resources.InstantiateZone()
