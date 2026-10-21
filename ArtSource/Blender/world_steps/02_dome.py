# World step 2 (route R4): the dome reactor. Run after bomber_steps/01_scene.py in an untitled session.
# Plinth, faceted dome with glowing teal seams between large panels, a door facing the game camera
# (Blender -Y = Unity -Z), side tanks, a mast with a rotating dish and a glowing tip.
# Moving parts (Dish, Tip) get their own bones in step 6; everything else follows the Root bone.
import bpy, bmesh, math
from mathutils import Vector, Matrix

scene = bpy.context.scene
PAL = bpy.data.materials["Palette"]
GLOW = bpy.data.materials["PaletteGlow"]


def uv_of(col, row):
    return ((col + 0.5) / 8.0, 1 - (row + 0.5) / 8.0)


def paint(bm, faces, col, row, glow=False):
    uv = bm.loops.layers.uv.verify()
    for f in faces:
        f.material_index = 1 if glow else 0
        for l in f.loops:
            l[uv].uv = uv_of(col, row)


def to_object(name, bm, location=(0, 0, 0)):
    old = bpy.data.objects.get(name)
    if old:
        m = old.data
        bpy.data.objects.remove(old, do_unlink=True)
        if m.users == 0:
            bpy.data.meshes.remove(m)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(PAL)
    me.materials.append(GLOW)
    for poly in me.polygons:
        poly.use_smooth = False
    ob = bpy.data.objects.new(name, me)
    ob.location = location
    scene.collection.objects.link(ob)
    return ob


# plinth: octagonal slab with a bevelled top and eight teal corner lights
bm = bmesh.new()
bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=8, radius1=1.95, radius2=1.85, depth=0.28)
bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(22.5), 3, "Z"))
bmesh.ops.translate(bm, vec=(0, 0, 0.14), verts=bm.verts)
bm.normal_update()
top = [f for f in bm.faces if f.normal.z > 0.9]
sides = [f for f in bm.faces if abs(f.normal.z) < 0.9]
paint(bm, sides, 2, 2)
inner = bmesh.ops.inset_region(bm, faces=top, thickness=0.18, depth=0.0)["faces"]
paint(bm, top, 3, 2)
paint(bm, inner, 1, 2)
for i in range(8):
    a = math.radians(22.5 + 45 * i)
    verts = bmesh.ops.create_cube(bm, size=0.12)["verts"]
    bmesh.ops.translate(bm, vec=(math.cos(a) * 1.82, math.sin(a) * 1.82, 0.3), verts=verts)
    paint(bm, list({f for v in verts for f in v.link_faces}), 4, 2, True)
to_object("Dome_Plinth", bm)

# dome: half icosphere, faces grouped by sector and ring into big panels, inset seams glow cyan
bm = bmesh.new()
bmesh.ops.create_icosphere(bm, subdivisions=2, radius=1.0)
bmesh.ops.delete(bm, geom=[v for v in bm.verts if v.co.z < -0.05], context="VERTS")
for v in bm.verts:
    v.co.z = max(v.co.z, 0.0)
    v.co = Vector((v.co.x * 1.45, v.co.y * 1.45, v.co.z * 1.2))
bmesh.ops.translate(bm, vec=(0, 0, 0.28), verts=bm.verts)
bm.normal_update()
groups = {}
for f in bm.faces:
    c = f.calc_center_median()
    h = (c.z - 0.28) / 1.2
    if h > 0.82:
        key = ("cap", 0)
    else:
        ring = 0 if h < 0.45 else 1
        sector = int(((math.atan2(c.y, c.x) + math.pi) / (2 * math.pi)) * (6 if ring == 0 else 5)) % (6 if ring == 0 else 5)
        key = (ring, sector)
    groups.setdefault(key, []).append(f)
panels = []
for key in groups:
    fs = groups[key]
    if len(fs) > 1:
        panels.extend(bmesh.ops.dissolve_faces(bm, faces=fs, use_verts=False)["region"])
    else:
        panels.extend(fs)
bm.normal_update()
before = set(bm.faces)
bmesh.ops.inset_individual(bm, faces=panels, thickness=0.035, depth=0.0)
seams = [f for f in bm.faces if f not in before]
for f in panels:
    n = f.normal.copy()
    for v in f.verts:
        v.co += n * 0.03
paint(bm, panels, 1, 2)
paint(bm, seams, 4, 2, True)
to_object("Dome_Shell", bm)

# door facing the camera (-Y): frame plus a glowing teal panel
bm = bmesh.new()
verts = bmesh.ops.create_cube(bm, size=1.0)["verts"]
bmesh.ops.scale(bm, vec=(0.62, 0.5, 0.78), verts=verts)
bmesh.ops.translate(bm, vec=(0, -1.33, 0.67), verts=verts)
bm.normal_update()
paint(bm, bm.faces, 2, 2)
front = [f for f in bm.faces if f.normal.y < -0.9]
door = bmesh.ops.inset_region(bm, faces=front, thickness=0.07, depth=-0.03)["faces"]
paint(bm, front, 4, 2, True)
to_object("Dome_Door", bm)

# side tanks with teal bands
bm = bmesh.new()
for sx in (-1, 1):
    r = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=6, radius1=0.26, radius2=0.26, depth=0.7)
    verts = r["verts"]
    bmesh.ops.translate(bm, vec=(sx * 1.55, 0.55, 0.63), verts=verts)
    faces = list({f for v in verts for f in v.link_faces})
    paint(bm, faces, 3, 2)
    band = bmesh.ops.create_cone(bm, cap_ends=False, segments=6, radius1=0.275, radius2=0.275, depth=0.08)["verts"]
    bmesh.ops.translate(bm, vec=(sx * 1.55, 0.55, 0.78), verts=band)
    paint(bm, list({f for v in band for f in v.link_faces}), 4, 2, True)
to_object("Dome_Tanks", bm)

# mast on top of the dome
bm = bmesh.new()
r = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=4, radius1=0.06, radius2=0.035, depth=1.0)
bmesh.ops.translate(bm, vec=(0, 0, 1.48 + 0.5), verts=r["verts"])
paint(bm, bm.faces, 3, 2)
to_object("Dome_Mast", bm)

# radar dish (rotates in Idle): a shallow cone on a short arm
bm = bmesh.new()
r = bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=6, radius1=0.3, radius2=0.06, depth=0.12)
bmesh.ops.rotate(bm, verts=r["verts"], cent=(0, 0, 0), matrix=Matrix.Rotation(math.radians(70), 3, "X"))
bmesh.ops.translate(bm, vec=(0, 0.16, 0.0), verts=r["verts"])
paint(bm, bm.faces, 1, 2)
arm = bmesh.ops.create_cube(bm, size=1.0)["verts"]
bmesh.ops.scale(bm, vec=(0.05, 0.16, 0.05), verts=arm)
bmesh.ops.translate(bm, vec=(0, 0.08, 0), verts=arm)
paint(bm, list({f for v in arm for f in v.link_faces}), 3, 2)
to_object("Dome_Dish", bm, location=(0, 0, 2.15))

# glowing mast tip (pulses in Idle)
bm = bmesh.new()
bmesh.ops.create_icosphere(bm, subdivisions=1, radius=0.09)
paint(bm, bm.faces, 6, 2, True)
to_object("Dome_Tip", bm, location=(0, 0, 2.5))

tris = 0
for ob in bpy.data.objects:
    if ob.name.startswith("Dome_"):
        tris += sum(len(p.vertices) - 2 for p in ob.data.polygons)
print("dome parts built, tris", tris)
