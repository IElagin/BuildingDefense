# Step 1 of the bug bomber build (route R4). Run through Blender MCP (execute_blender_code) in an
# untitled session: bpy.ops.wm.read_homefile(use_empty=True) first, so the FBX carries no .blend path.
import bpy, math

scene = bpy.context.scene
scene.render.fps = 24
scene.view_settings.view_transform = "Standard"
img = bpy.data.images.load("/Users/ilyaelagin/GameProject/BuildingDefense/ArtSource/Palette/palette.png", check_existing=True)


def palette_material(name, emission):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    tex = nt.nodes.new("ShaderNodeTexImage")
    tex.image = img
    tex.interpolation = "Closest"
    nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value = 0.85
    if emission:
        em = bsdf.inputs.get("Emission Color") or bsdf.inputs.get("Emission")
        nt.links.new(tex.outputs["Color"], em)
        bsdf.inputs["Emission Strength"].default_value = 1.5
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    return mat


pal = palette_material("Palette", False)
palette_material("PaletteGlow", True)

# preview-only ground, sun and game camera; never exported
me = bpy.data.meshes.new("Preview_Ground")
s = 6.0
me.from_pydata([(-s, -s, 0), (s, -s, 0), (s, s, 0), (-s, s, 0)], [], [(0, 1, 2, 3)])
uv = me.uv_layers.new(name="UVMap")
for loop in uv.data:
    loop.uv = ((2 + 0.5) / 8.0, 1.0 - (1 + 0.5) / 8.0)
me.materials.append(pal)
scene.collection.objects.link(bpy.data.objects.new("Preview_Ground", me))
sun = bpy.data.objects.new("Preview_Sun", bpy.data.lights.new("Preview_Sun", "SUN"))
sun.data.energy = 3.0
sun.data.angle = math.radians(8)
scene.collection.objects.link(sun)
sun.rotation_euler = (math.radians(50), math.radians(10), math.radians(30))
world = bpy.data.worlds.new("World")
scene.world = world
world.use_nodes = True
bg = next(n for n in world.node_tree.nodes if n.type == "BACKGROUND")
bg.inputs["Color"].default_value = (0.55, 0.5, 0.55, 1)
bg.inputs["Strength"].default_value = 0.6
cam = bpy.data.objects.new("Preview_GameCamera", bpy.data.cameras.new("Preview_GameCamera"))
cam.data.type = "ORTHO"
cam.data.ortho_scale = 4.0
scene.collection.objects.link(cam)
d = 20.0
cam.location = (0, -d * math.cos(math.radians(55)), d * math.sin(math.radians(55)))
cam.rotation_euler = (math.radians(35), 0, 0)
scene.camera = cam
scene.render.resolution_x, scene.render.resolution_y = 1280, 720
print("scene ready")
