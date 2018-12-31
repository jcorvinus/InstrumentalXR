using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Instrumental.Schema;

namespace Instrumental.Editing
{
    [CustomEditor(typeof(PanelEditor))]
    public class PanelEditorEditor : Editor
    {
        string defaultName = "UIpanel";
        PanelEditor m_instance;
        SerializedProperty uiSchemaProperty;

        private void OnEnable()
        {
            m_instance = target as PanelEditor;
            uiSchemaProperty = serializedObject.FindProperty("uiSchema");
        }

        public override void OnInspectorGUI()
        {
            if (uiSchemaProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create new panel"))
                {
                    string savePath = EditorUtility.SaveFilePanelInProject(
                        "Save Panel File To ..", defaultName, "asset", "please select a folder and enter the file name");

                    // get our dialog box and path
                    if(Directory.Exists(Path.GetDirectoryName(savePath)))
                    {
                        UISchema uiSchema = ScriptableObject.CreateInstance<UISchema>();
                        uiSchema.Panel = PanelSchema.GetDefaults();
                        if(!File.Exists(savePath)) AssetDatabase.CreateAsset(uiSchema, savePath); // don't save over existing file

                        serializedObject.Update();
                        uiSchemaProperty.objectReferenceValue = uiSchema;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                base.OnInspectorGUI();

                if (Application.isPlaying)
                {
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Save Layout"))
                    {
                        string savePath = EditorUtility.SaveFilePanelInProject(
                            "Save Panel File To ..", defaultName, "asset", "please select a folder and enter the file name");

                        // get our dialog box and path
                        if (Directory.Exists(Path.GetDirectoryName(savePath)))
                        {
                            m_instance.Save();
                            UISchema uiSchema = (UISchema)uiSchemaProperty.objectReferenceValue;
                            if(!File.Exists(savePath)) AssetDatabase.CreateAsset(uiSchema, savePath);

                            serializedObject.Update();
                            /*uiSchemaProperty.objectReferenceValue = uiSchema;
                            serializedObject.ApplyModifiedProperties();*/
                        }
                    }
                }
            }
        }
    }
}