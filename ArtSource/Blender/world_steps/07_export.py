# World step 7: save the author copy (world.blend) and export one FBX per asset from the untitled
# session (no .blend path in the FBX). The palette image is unlinked during export so the FBX holds
# only the Palette/PaletteGlow slots. Rigs export only their own current action (Dome: Idle,
# Mine: Armed) with bake_space_transform off; static meshes bake the axes like the L7 models.
# Every asset is exported at the origin and put back afterwards. Scan the FBX files for "/Users/" after.
import bpy

assert bpy.data.filepath == "", "export from an untitled session"
MODELS = "/Users/ilyaelagin/GameProject/BuildingDefense/Assets/_Project/Art/Models/"
bpy.data.images["palette.png"].filepath = "//../Palette/palette.png"  # relative to world.blend
bpy.ops.wm.save_as_mainfile(filepath="/Users/ilyaelagin/GameProject/BuildingDefense/ArtSource/Blender/world.blend", copy=True)
bpy.data.images["palette.png"].filepath = "/Users/ilyaelagin/GameProject/BuildingDefense/ArtSource/Palette/palette.png"

saved = []
for mat in (bpy.data.materials["Palette"], bpy.data.materials["PaletteGlow"]):
    nt = mat.node_tree
    for link in list(nt.links):
        if link.from_node.type == "TEX_IMAGE":
            saved.append((mat.name, link.from_socket.name, link.to_node.name, link.to_socket.identifier))
            nt.links.remove(link)


def select_only(objects):
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    for o in objects:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]


def export_rig(name, action, file_name):
    arm = bpy.data.objects[name]
    old_location = arm.location.copy()
    arm.location = (0, 0, 0)
    arm.animation_data.action = bpy.data.actions[action]
    scene = bpy.context.scene
    old_range = (scene.frame_start, scene.frame_end)
    # a single-action export takes the scene range and name: bound the range to the action
    scene.frame_start, scene.frame_end = (int(f) for f in bpy.data.actions[action].frame_range)
    scene.frame_set(scene.frame_start)
    select_only([arm] + list(arm.children))
    bpy.ops.export_scene.fbx(
        filepath=MODELS + file_name + ".fbx", use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False, bake_space_transform=False,
        use_armature_deform_only=False, armature_nodetype="NULL",
        bake_anim=True, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=False, bake_anim_use_all_actions=False,
        bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
        path_mode="STRIP", embed_textures=False, use_metadata=False)
    arm.animation_data.action = None
    for pb in arm.pose.bones:
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)
        pb.scale = (1, 1, 1)
    arm.location = old_location
    scene.frame_start, scene.frame_end = old_range


def export_static(name):
    ob = bpy.data.objects[name]
    old_location = ob.location.copy()
    ob.location = (0, 0, 0)
    select_only([ob])
    bpy.ops.export_scene.fbx(
        filepath=MODELS + name + ".fbx", use_selection=True, object_types={"MESH"},
        apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False, bake_space_transform=True,
        bake_anim=False, path_mode="STRIP", embed_textures=False, use_metadata=False)
    ob.location = old_location


export_rig("Dome", "Idle", "Dome")
export_rig("Mine", "Armed", "LandMine")
for name in ("Arena", "Rock_A", "Rock_B", "Rock_C", "Crystal_A", "Crystal_B", "Burrow"):
    export_static(name)

for mname, from_sock, to_node, to_sock in saved:
    nt = bpy.data.materials[mname].node_tree
    tex = next(n for n in nt.nodes if n.type == "TEX_IMAGE")
    target = next(s for s in nt.nodes[to_node].inputs if s.identifier == to_sock)
    nt.links.new(tex.outputs[from_sock], target)
print("exported world assets")
