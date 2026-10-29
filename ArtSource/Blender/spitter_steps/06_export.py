# Spitter step 6: author copy spitter.blend, then BugSpitter.fbx (the armature renamed "Spitter") and AcidSpit.fbx
# (a glowing faceted blob, built here, exported alone). Same rules as bomber_steps/06_export.py: the session stays
# untitled and the palette image is unlinked for the export. After export: scan both FBX for "/Users/" (expect 0 hits).
import bpy, bmesh

assert bpy.data.filepath == "", "export from an untitled session"
MODELS = "/Users/ilyaelagin/GameProject/BuildingDefense/Assets/_Project/Art/Models/"
arm = bpy.data.objects["Bomber"]
arm.name = "Spitter"
arm.data.name = "SpitterRig"

# the acid blob, the projectile visual: 0.32 m long along +Y (the flight direction after export is +Z)
old = bpy.data.objects.get("AcidSpit")
if old:
    bpy.data.objects.remove(old)
bm = bmesh.new()
bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)
bmesh.ops.scale(bm, vec=(0.11, 0.16, 0.11), verts=bm.verts)
uv = bm.loops.layers.uv.verify()
for f in bm.faces:
    f.material_index = 1
    tail = f.calc_center_median().y < -0.06
    for l in f.loops:
        l[uv].uv = ((5.5 if tail else 4.5) / 8.0, 1 - 0.5 / 8.0)
me = bpy.data.meshes.new("AcidSpit")
bm.to_mesh(me)
bm.free()
me.materials.append(bpy.data.materials["Palette"])
me.materials.append(bpy.data.materials["PaletteGlow"])
blob = bpy.data.objects.new("AcidSpit", me)
blob.location = (1.5, 0, 0.3)
bpy.context.scene.collection.objects.link(blob)

bpy.data.images["palette.png"].filepath = "//../Palette/palette.png"
bpy.ops.wm.save_as_mainfile(filepath="/Users/ilyaelagin/GameProject/BuildingDefense/ArtSource/Blender/spitter.blend", copy=True)
saved = []
for mat in (bpy.data.materials["Palette"], bpy.data.materials["PaletteGlow"]):
    nt = mat.node_tree
    for link in list(nt.links):
        if link.from_node.type == "TEX_IMAGE":
            saved.append((mat.name, link.from_socket.name, link.to_node.name, link.to_socket.identifier))
            nt.links.remove(link)


def select(objs):
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]


COMMON = dict(apply_scale_options="FBX_SCALE_UNITS", axis_forward="-Z", axis_up="Y", use_mesh_modifiers=True,
              mesh_smooth_type="FACE", add_leaf_bones=False, bake_space_transform=False, path_mode="STRIP",
              embed_textures=False, use_metadata=False, use_selection=True)
bpy.context.scene.frame_set(1)
select([arm] + list(arm.children))
bpy.ops.export_scene.fbx(
    filepath=MODELS + "BugSpitter.fbx", object_types={"ARMATURE", "MESH"},
    use_armature_deform_only=False, armature_nodetype="NULL",
    bake_anim=True, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=False, bake_anim_use_all_actions=True,
    bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0, **COMMON)
loc = tuple(blob.location)
blob.location = (0, 0, 0)
select([blob])
bpy.ops.export_scene.fbx(filepath=MODELS + "AcidSpit.fbx", object_types={"MESH"}, bake_anim=False, **COMMON)
blob.location = loc
for mname, from_sock, to_node, to_sock in saved:
    nt = bpy.data.materials[mname].node_tree
    tex = next(n for n in nt.nodes if n.type == "TEX_IMAGE")
    target = next(s for s in nt.nodes[to_node].inputs if s.identifier == to_sock)
    nt.links.new(tex.outputs[from_sock], target)
tris = sum(sum(len(p.vertices) - 2 for p in c.data.polygons) for c in arm.children if c.type == "MESH")
print("exported", len(arm.children), "parts", tris, "tris", [a.name for a in bpy.data.actions])
