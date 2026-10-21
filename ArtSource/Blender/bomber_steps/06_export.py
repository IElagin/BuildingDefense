# Step 6: keep an author copy without giving the session a file path, then export the FBX.
# The session must stay untitled (bpy.data.filepath == "") or the FBX records the .blend path
# in Original|ApplicationNativeFile. Even with path_mode STRIP the exporter writes an absolute
# texture FileName, so the palette image is unlinked for the export: the FBX carries only the
# Palette/PaletteGlow slots, which Unity remaps to its own palette materials.
# After export: scan the FBX for "/Users/" (expect 0 hits).
import bpy

assert bpy.data.filepath == "", "export from an untitled session"
bpy.data.images["palette.png"].filepath = "//../Palette/palette.png"  # relative to bugs.blend
bpy.ops.wm.save_as_mainfile(filepath="/Users/ilyaelagin/GameProject/BuildingDefense/ArtSource/Blender/bugs.blend", copy=True)
saved = []
for mat in (bpy.data.materials["Palette"], bpy.data.materials["PaletteGlow"]):
    nt = mat.node_tree
    for link in list(nt.links):
        if link.from_node.type == "TEX_IMAGE":
            saved.append((mat.name, link.from_socket.name, link.to_node.name, link.to_socket.identifier))
            nt.links.remove(link)
arm = bpy.data.objects["Bomber"]
for o in bpy.context.view_layer.objects:
    o.select_set(False)
arm.select_set(True)
for c in arm.children:
    c.select_set(True)
bpy.context.view_layer.objects.active = arm
bpy.context.scene.frame_set(1)
bpy.ops.export_scene.fbx(
    filepath="/Users/ilyaelagin/GameProject/BuildingDefense/Assets/_Project/Art/Models/BugBomber.fbx",
    use_selection=True, object_types={"ARMATURE", "MESH"},
    apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
    use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False, bake_space_transform=False,
    use_armature_deform_only=False, armature_nodetype="NULL",
    bake_anim=True, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=False, bake_anim_use_all_actions=True,
    bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
    path_mode="STRIP", embed_textures=False, use_metadata=False)
for mname, from_sock, to_node, to_sock in saved:
    nt = bpy.data.materials[mname].node_tree
    tex = next(n for n in nt.nodes if n.type == "TEX_IMAGE")
    target = next(s for s in nt.nodes[to_node].inputs if s.identifier == to_sock)
    nt.links.new(tex.outputs[from_sock], target)
tris =sum(sum(len(p.vertices) - 2 for p in c.data.polygons) for c in arm.children if c.type == "MESH")
print("exported", len(arm.children), "parts", tris, "tris", [a.name for a in bpy.data.actions])
