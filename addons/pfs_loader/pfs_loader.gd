@tool
extends EditorPlugin

var loader
var importer
var scene_importer

func _enter_tree() -> void:
	# Initialization of the plugin goes here.
	print("Initializing pfs_loader")
#	loader = EqPackFileResourceLoader.new() 
#	ResourceLoader.add_resource_format_loader(loader)
# 	importer = EqPackFileImporter.new()
# 	add_import_plugin(importer)
# 	scene_importer = EqEditorSceneImporter.new()
# 	add_scene_format_importer_plugin(scene_importer)
	
func _exit_tree() -> void:
	# Clean-up of the plugin goes here.
	print("Cleanup pfs_loader")
	if loader != null:
		ResourceLoader.remove_resource_format_loader(loader)
		loader = null
	if scene_importer != null:
		remove_scene_format_importer_plugin(scene_importer)
		scene_importer = null
	if importer != null:
		remove_import_plugin(importer)
		importer = null
