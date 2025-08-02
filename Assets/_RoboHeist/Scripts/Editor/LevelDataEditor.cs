using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LevelGridData))]
public class LevelDataEditor : PropertyDrawer
{
    private const int cellSize = 32;
    private const int cellMargin = 0;
    private GUIStyle buttonStyle = null;
    private List<RobotConfig> robotConfigurations = null;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var dimensionsProp = property.FindPropertyRelative("Dimensions");
        int height = dimensionsProp.vector2IntValue.y;

        return EditorGUIUtility.singleLineHeight + (cellSize + cellMargin) * height + EditorGUIUtility.singleLineHeight * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (buttonStyle == null)
        {
            var noOffset = new RectOffset(0, 0, 0, 0);
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.border = noOffset;
            buttonStyle.margin = noOffset;
            buttonStyle.padding = noOffset;
        }

        if (robotConfigurations == null)
        {
            robotConfigurations = FindAllInstances<RobotConfig>();
        }

        string propertyLabelText = label.text;
        var dimensionsProp = property.FindPropertyRelative("Dimensions");

        // Ensure data list has correct size
        SerializedObject so = property.serializedObject;

        EditorGUI.BeginProperty(position, label, property);

        Rect d = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginChangeCheck();
        dimensionsProp.vector2IntValue = EditorGUI.Vector2IntField(d, "Level Size", dimensionsProp.vector2IntValue);
        var dimensions = dimensionsProp.vector2IntValue;

        if (EditorGUI.EndChangeCheck())
        {
            dimensionsProp.serializedObject.ApplyModifiedProperties();
        }

        if (so.isEditingMultipleObjects == false)
        {
            var dataProp = property.FindPropertyRelative("TileEntities");
            var gridObj = fieldInfo.GetValue(so.targetObject) as LevelGridData;
            gridObj?.EnsureSize();

            Rect r = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(r, propertyLabelText);
            r.y += EditorGUIUtility.singleLineHeight + 4;

            if (dataProp != null)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    r.x = position.x;
                    for (int x = 0; x < dimensions.x; x++)
                    {
                        int index = y * dimensions.x + x;
                        var tileProp = dataProp.GetArrayElementAtIndex(index);
                        var tileEntityProp = tileProp.FindPropertyRelative("TileEntity");
                        var tileEntity = tileEntityProp.managedReferenceValue as TileEntityData;
                        //var icon = new GUIContent(CompositeIcons(GetFloorIcon(gridLocation.floor), GetIcon(gridLocation.TileEntity)), $"{tileProp.GetType()}");
                        var icon = new GUIContent(GetIcon(tileEntity), $"{tileProp.GetType()}");

                        var iconRect = new Rect(r.x, r.y, cellSize, cellSize);

                        // Rotate the icon to face the correct direction:
                        Matrix4x4 oldMatrix = GUI.matrix;
                        if (tileEntity != null)
                        {
                            Vector2 pivot = new Vector2(iconRect.x + iconRect.width / 2f, iconRect.y + iconRect.height / 2f);
                            GUIUtility.RotateAroundPivot(GetFacingAngle(tileEntity.direction), pivot);
                        }

                        if (GUI.Button(iconRect, icon, buttonStyle))
                        {
                            GenericMenu menu = new GenericMenu();

                            if (tileEntity != null)
                            {
                                menu.AddItem(new GUIContent("Face Up"), false, () =>
                                {
                                    ((TileEntityData)tileEntityProp.managedReferenceValue).direction = Direction.Up;
                                    tileEntityProp.serializedObject.ApplyModifiedProperties();
                                });
                                menu.AddItem(new GUIContent("Face Down"), false, () =>
                                {
                                    ((TileEntityData)tileEntityProp.managedReferenceValue).direction = Direction.Down;
                                    tileEntityProp.serializedObject.ApplyModifiedProperties();
                                });
                                menu.AddItem(new GUIContent("Face Left"), false, () =>
                                {
                                    ((TileEntityData)tileEntityProp.managedReferenceValue).direction = Direction.Left;
                                    tileEntityProp.serializedObject.ApplyModifiedProperties();
                                });
                                menu.AddItem(new GUIContent("Face Right"), false, () =>
                                {
                                    ((TileEntityData)tileEntityProp.managedReferenceValue).direction = Direction.Right;
                                    tileEntityProp.serializedObject.ApplyModifiedProperties();
                                });

                                menu.AddSeparator("");
                            }

                            menu.AddItem(new GUIContent("Nothing"), tileEntity == null, () =>
                            {
                                tileEntityProp.managedReferenceValue = null;
                                tileEntityProp.serializedObject.ApplyModifiedProperties();
                            });
                            menu.AddItem(new GUIContent("Wall"), tileEntity?.GetType() == typeof(WallEntityData), () =>
                            {
                                tileEntityProp.managedReferenceValue = new WallEntityData();
                                tileEntityProp.serializedObject.ApplyModifiedProperties();
                            });
                            menu.AddItem(new GUIContent("Gold!"), tileEntity?.GetType() == typeof(GoldEntityData), () =>
                            {
                                tileEntityProp.managedReferenceValue = new GoldEntityData();
                                tileEntityProp.serializedObject.ApplyModifiedProperties();
                            });
                            menu.AddItem(new GUIContent("Treasure Target"), tileEntity?.GetType() == typeof(TreasureTargetEntityData), () =>
                            {
                                tileEntityProp.managedReferenceValue = new TreasureTargetEntityData();
                                tileEntityProp.serializedObject.ApplyModifiedProperties();
                            });

                            foreach (var config in robotConfigurations)
                            {
                                var configToCapture = config;
                                menu.AddItem(new GUIContent($"{config.name} (Robot)"), tileEntity?.GetType() == typeof(RobotEntityData) && ((RobotEntityData)tileEntity).robotConfig == config, () =>
                                {
                                    var entityData = new RobotEntityData();
                                    entityData.robotConfig = configToCapture;
                                    tileEntityProp.managedReferenceValue = entityData;
                                    tileEntityProp.serializedObject.ApplyModifiedProperties();
                                });
                            }
                            menu.ShowAsContext(); // Shows the popup menu at mouse position
                        }

                        // Revert to the old matrix.
                        GUI.matrix = oldMatrix;


                        r.x += cellSize + cellMargin;
                    }

                    r.y += cellSize + cellMargin;
                }
            }
        }

        EditorGUI.EndProperty();
    }

    private float GetFacingAngle(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return 180.0f;
            case Direction.Down: return 0.0f;
            case Direction.Left: return 90.0f;
            case Direction.Right: return -90.0f;
            default: return 0.0f;
        }
    }

    private Texture2D GetFloorIcon(string typeName)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_RoboHeist/Scripts/Editor/ico_floor.png");
    }

    private Texture2D GetIcon(TileEntityData tileEntity)
    {
        if (tileEntity == null)
        {
            return null;
        }

        var texture = tileEntity switch
        {
            RobotEntityData r when r.robotConfig == robotConfigurations[0] => AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_RoboHeist/Scripts/Editor/ico_forky.png"),
            RobotEntityData r when r.robotConfig == robotConfigurations[1] => AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_RoboHeist/Scripts/Editor/ico_plod.png"),
            WallEntityData _ => AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_RoboHeist/Scripts/Editor/ico_wall.png"),
            _ => null
        };

        return texture;
    }

    private Texture2D CompositeIcons(Texture2D baseIcon, Texture2D overlayIcon)
    {
        Texture2D result = new Texture2D(baseIcon.width, baseIcon.height, TextureFormat.RGBA32, false);

        // Copy base icon
        Color[] basePixels = baseIcon.GetPixels();
        result.SetPixels(basePixels);

        // Blend overlay (e.g., alpha-blend)
        Color[] overlayPixels = overlayIcon.GetPixels();
        for (int i = 0; i < overlayPixels.Length; i++)
        {
            Color src = overlayPixels[i];
            Color dst = result.GetPixel(i % baseIcon.width, i / baseIcon.width);
            result.SetPixel(i % baseIcon.width, i / baseIcon.width, Color.Lerp(dst, src, src.a));
        }

        result.Apply();
        return result;
    }

    public static List<T> FindAllInstances<T>() where T : ScriptableObject
    {
        List<T> results = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                results.Add(asset);
            }
        }

        return results;
    }
}
