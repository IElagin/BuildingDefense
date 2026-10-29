# Defense step 3: save the author copy (defense.blend) and export the FBX files from the untitled session, as
# world_steps/07_export.py does: the palette image is unlinked during export, static meshes bake the axes, every
# asset goes out at the origin and is put back afterwards. Turret.fbx holds both parts (Turret_Base, Turret_Head).
import bpy

assert bpy.data.filepath == "", "export from an untitled session"
MODELS = "/Users/ilyaelagin/GameProject/BuildingDefense/Assets/_Project/Art/Models/"
image = next(i for i in bpy.data.images if i.filepath.endswith("palette.png"))
absolute = image.filepath
image.filepath = "//../Palette/palette.png"  # relative to defense.blend
bpy.ops.wm.save_as_mainfile(filepath="/Users/ilyaelagin/GameProject/BuildingDefense/ArtSource/Blender/defense.blend", copy=True)
image.filepath = absolute

saved = []
for mat in (bpy.data.materials["Palette"], bpy.data.materials["PaletteGlow"]):
    nt = mat.node_tree
    for link in list(nt.links):
        if link.from_node.type == "TEX_IMAGE":
            saved.append((mat.name, link.from_socket.name, link.to_node.name, link.to_socket.identifier))
            nt.links.remove(link)


def export_static(file_name, names):
    objects = [bpy.data.objects[n] for n in names]
    old = [o.location.copy() for o in objects]
    for o in objects:
        o.location = (0, 0, 0)
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    for o in objects:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(
        filepath=MODELS + file_name + ".fbx", use_selection=True, object_types={"MESH"},
        apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False, bake_space_transform=True,
        bake_anim=False, path_mode="STRIP", embed_textures=False, use_metadata=False)
    for o, location in zip(objects, old):
        o.location = location


export_static("Turret", ["Turret_Base", "Turret_Head"])
export_static("Puddle", ["Puddle"])
export_static("PlasmaBolt", ["PlasmaBolt"])

for mname, from_sock, to_node, to_sock in saved:
    nt = bpy.data.materials[mname].node_tree
    tex = next(n for n in nt.nodes if n.type == "TEX_IMAGE")
    target = next(s for s in nt.nodes[to_node].inputs if s.identifier == to_sock)
    nt.links.new(tex.outputs[from_sock], target)
print("exported defense assets")
