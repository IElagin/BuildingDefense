"""Builds BuildingDefense low-poly placeholder models and exports them to the Unity project.

Run headless from any directory (no Blender window, no MCP port):
  /Applications/Blender.app/Contents/MacOS/Blender -b --factory-startup --python build_models.py -- <project-root>
Blender 4.5 LTS. Meters, origin on the ground. With this export Blender +Y becomes Unity +Z (forward):
the bomber faces +Y (it walks forward), the tower door faces -Y (towards the game camera).
Writes Assets/_Project/Art/Models/*.fbx, ArtSource/Blender/models.blend and previews/*.png.
"""
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector

PROJECT = Path(sys.argv[sys.argv.index('--') + 1]).resolve()
MODELS_DIR = PROJECT / 'Assets/_Project/Art/Models'
SOURCE_DIR = PROJECT / 'ArtSource/Blender'
PREVIEWS_DIR = SOURCE_DIR / 'previews'

PALETTE = {
    'BD_StoneLight': (0.62, 0.60, 0.56),
    'BD_StoneDark': (0.36, 0.35, 0.34),
    'BD_Roof': (0.62, 0.16, 0.12),
    'BD_Wood': (0.40, 0.24, 0.12),
    'BD_Flag': (0.95, 0.80, 0.15),
    'BD_Window': (0.08, 0.08, 0.10),
    'BD_BomberSkin': (0.38, 0.62, 0.22),
    'BD_EyeWhite': (0.95, 0.95, 0.92),
    'BD_EyePupil': (0.05, 0.05, 0.05),
    'BD_Bomb': (0.10, 0.10, 0.12),
    'BD_Fuse': (0.75, 0.62, 0.40),
    'BD_Spark': (1.00, 0.55, 0.05),
    'BD_MineMetal': (0.25, 0.27, 0.28),
    'BD_MineLight': (1.00, 0.12, 0.08),
    'BD_Rock': (0.48, 0.47, 0.46),
    'BD_Trunk': (0.35, 0.22, 0.12),
    'BD_Leaves': (0.18, 0.45, 0.20),
}


def material(name):
    found = bpy.data.materials.get(name)
    if found:
        return found
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    color = (*PALETTE[name], 1.0)
    mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = color
    mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value = 0.85
    mat.diffuse_color = color
    return mat


def finish(obj, mat_name):
    obj.data.materials.append(material(mat_name))
    for poly in obj.data.polygons:
        poly.use_smooth = False
    return obj


def cylinder(name, radius, depth, z, mat, vertices=10, top_radius=None, x=0.0, y=0.0):
    if top_radius is None:
        bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=(x, y, z + depth / 2))
    else:
        bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius, radius2=top_radius, depth=depth, location=(x, y, z + depth / 2))
    obj = bpy.context.active_object
    obj.name = name
    return finish(obj, mat)


def box(name, size, location, mat, rotation_z=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=(0, 0, rotation_z))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    return finish(obj, mat)


def sphere(name, radius, location, mat, segments=8, rings=6, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=radius, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    return finish(obj, mat)


def set_origin_to_ground(obj):
    # The pivot is the world origin, so the model stands on the ground at its own position.
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def join(name, parts):
    bpy.ops.object.select_all(action='DESELECT')
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name
    set_origin_to_ground(obj)
    return obj


def build_tower():
    parts = [
        cylinder('Base', 1.55, 0.45, 0.0, 'BD_StoneDark', vertices=12),
        cylinder('Body', 1.3, 3.0, 0.45, 'BD_StoneLight', vertices=12, top_radius=1.15),
        cylinder('Parapet', 1.45, 0.45, 3.45, 'BD_StoneDark', vertices=12),
    ]
    for index in range(10):
        angle = index / 10 * math.tau
        parts.append(box('Merlon', (0.42, 0.32, 0.45), (math.cos(angle) * 1.3, math.sin(angle) * 1.3, 4.12), 'BD_StoneLight', rotation_z=angle))
    parts.append(cylinder('RoofDrum', 0.9, 0.5, 3.9, 'BD_StoneLight', vertices=10))
    parts.append(cylinder('Roof', 1.05, 1.5, 4.4, 'BD_Roof', vertices=10, top_radius=0.0))
    parts.append(cylinder('Pole', 0.04, 0.9, 5.85, 'BD_Wood', vertices=6))
    parts.append(box('Flag', (0.55, 0.04, 0.32), (0.3, 0.0, 6.55), 'BD_Flag'))
    parts.append(box('Door', (0.6, 0.12, 1.0), (0.0, -1.33, 0.95), 'BD_Wood'))
    parts.append(box('DoorTop', (0.42, 0.1, 0.18), (0.0, -1.33, 1.5), 'BD_Wood'))
    for z, angle in ((2.4, math.radians(-90)), (2.9, math.radians(30)), (2.9, math.radians(150))):
        x, y = math.cos(angle) * 1.22, math.sin(angle) * 1.22
        parts.append(box('Window', (0.22, 0.14, 0.42), (x, y, z), 'BD_Window', rotation_z=angle + math.pi / 2))
    return join('Tower', parts)


def build_bomber():
    parts = [
        sphere('Body', 0.36, (0, 0, 0.42), 'BD_BomberSkin', segments=10, rings=7, scale=(1.0, 0.9, 1.05)),
        box('FootL', (0.16, 0.24, 0.1), (-0.15, -0.05, 0.05), 'BD_Bomb'),
        box('FootR', (0.16, 0.24, 0.1), (0.15, -0.05, 0.05), 'BD_Bomb'),
        sphere('EyeL', 0.1, (-0.13, -0.29, 0.55), 'BD_EyeWhite', segments=6, rings=4),
        sphere('EyeR', 0.1, (0.13, -0.29, 0.55), 'BD_EyeWhite', segments=6, rings=4),
        sphere('PupilL', 0.045, (-0.13, -0.38, 0.55), 'BD_EyePupil', segments=6, rings=4),
        sphere('PupilR', 0.045, (0.13, -0.38, 0.55), 'BD_EyePupil', segments=6, rings=4),
        cylinder('EarL', 0.09, 0.22, 0.7, 'BD_BomberSkin', vertices=5, top_radius=0.0, x=-0.24),
        cylinder('EarR', 0.09, 0.22, 0.7, 'BD_BomberSkin', vertices=5, top_radius=0.0, x=0.24),
        sphere('Bomb', 0.24, (0, 0.28, 0.72), 'BD_Bomb', segments=8, rings=6),
        cylinder('Fuse', 0.035, 0.22, 0.92, 'BD_Fuse', vertices=6, y=0.3),
        sphere('Spark', 0.07, (0, 0.3, 1.17), 'BD_Spark', segments=6, rings=4),
    ]
    bomber = join('Bomber', parts)
    # Built facing -Y like the other parts; turn it to face +Y, which is Unity's forward.
    bomber.rotation_euler = (0, 0, math.pi)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    return bomber


def build_mine():
    parts = [
        cylinder('Plate', 0.42, 0.1, 0.0, 'BD_MineMetal', vertices=10, top_radius=0.36),
        cylinder('Cap', 0.22, 0.08, 0.1, 'BD_Bomb', vertices=8),
        sphere('Light', 0.09, (0, 0, 0.2), 'BD_MineLight', segments=6, rings=4),
    ]
    for index in range(4):
        angle = index / 4 * math.tau + math.pi / 4
        parts.append(cylinder('Spike', 0.05, 0.12, 0.08, 'BD_MineMetal', vertices=5, top_radius=0.0, x=math.cos(angle) * 0.3, y=math.sin(angle) * 0.3))
    return join('Mine', parts)


def build_rock():
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.6, location=(0, 0, 0.3))
    rock = bpy.context.active_object
    rock.name = 'Rock'
    for vertex in rock.data.vertices:
        seed = math.sin(vertex.co.x * 12.9898 + vertex.co.y * 78.233 + vertex.co.z * 37.719) * 43758.5453
        jitter = 0.82 + (seed - math.floor(seed)) * 0.36
        vertex.co *= jitter
    rock.scale = (1.2, 0.9, 0.7)
    finish(rock, 'BD_Rock')
    set_origin_to_ground(rock)
    return rock


def build_tree():
    parts = [
        cylinder('Trunk', 0.14, 0.7, 0.0, 'BD_Trunk', vertices=6),
        cylinder('LeavesLow', 0.75, 1.0, 0.6, 'BD_Leaves', vertices=7, top_radius=0.2),
        cylinder('LeavesHigh', 0.55, 0.9, 1.3, 'BD_Leaves', vertices=7, top_radius=0.0),
    ]
    return join('Tree', parts)


def export(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    path = MODELS_DIR / (obj.name + '.fbx')
    bpy.ops.export_scene.fbx(
        filepath=str(path), use_selection=True, object_types={'MESH'},
        apply_scale_options='FBX_SCALE_UNITS', axis_forward='-Z', axis_up='Y',
        use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False, bake_space_transform=True,
        bake_anim=False, use_metadata=False)
    triangles = sum(len(p.vertices) - 2 for p in obj.data.polygons)
    size = obj.dimensions
    print(f'EXPORTED {path.name} tris={triangles} size=({size.x:.2f},{size.y:.2f},{size.z:.2f}) slots={len(obj.material_slots)}')


def render_preview(obj, others):
    for other in others:
        other.hide_render = other is not obj
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'MATERIAL'
    scene.display.shading.show_shadows = True
    scene.render.resolution_x = 512
    scene.render.resolution_y = 512
    scene.render.film_transparent = False
    height = max(obj.dimensions.z, 0.5)
    camera = bpy.data.objects.get('PreviewCamera')
    if camera is None:
        bpy.ops.object.camera_add()
        camera = bpy.context.active_object
        camera.name = 'PreviewCamera'
    scene.camera = camera
    distance = max(obj.dimensions) * 2.2 + 0.5
    # In front of the object and above it, like the game camera.
    front = 1 if obj.name == 'Bomber' else -1
    camera.location = Vector((distance * 0.35, front * distance * 0.8, height * 0.5 + distance * 0.75))
    target = Vector((0, 0, height * 0.45))
    camera.rotation_euler = (target - camera.location).to_track_quat('-Z', 'Y').to_euler()
    scene.render.filepath = str(PREVIEWS_DIR / (obj.name + '.png'))
    bpy.ops.render.render(write_still=True)


def main():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    MODELS_DIR.mkdir(parents=True, exist_ok=True)
    PREVIEWS_DIR.mkdir(parents=True, exist_ok=True)
    models = [build_tower(), build_bomber(), build_mine(), build_rock(), build_tree()]
    # Untitled session: the FBX carries no .blend path (Blender-To-Unity, export step 6).
    assert bpy.data.filepath == ''
    for obj in models:
        export(obj)
    for obj in models:
        render_preview(obj, models)
    bpy.context.scene.render.filepath = '//previews/'
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_DIR / 'models.blend'), copy=True)


main()
