using UnityEngine;
using UnityEditor;

public class RemoveArmature
{
    [MenuItem("Tools/Remove Armature Prefix")]
    static void RemovePrefix()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;

        if (clip == null)
        {
            Debug.LogError("Selecciona un AnimationClip");
            return;
        }

        var bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);

            var newBinding = binding;
            newBinding.path = binding.path.Replace("Armature/", "");

            AnimationUtility.SetEditorCurve(clip, binding, null);
            AnimationUtility.SetEditorCurve(clip, newBinding, curve);
        }

        Debug.Log("Se removió Armature/ de los paths");
    }
}
public class AddArmature
{
    [MenuItem("Tools/Add Armature Prefix")]
    static void AddPrefix()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;

        if (clip == null)
        {
            Debug.LogError("Selecciona un AnimationClip");
            return;
        }

        var bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (var binding in bindings)
        {
            // No tocar paths vacíos
            if (string.IsNullOrEmpty(binding.path))
                continue;

            // No tocar si ya tiene Armature al inicio
            if (binding.path.StartsWith("Armature"))
                continue;

            var curve = AnimationUtility.GetEditorCurve(clip, binding);

            var newBinding = binding;
            newBinding.path = "Armature/" + binding.path;

            AnimationUtility.SetEditorCurve(clip, binding, null);
            AnimationUtility.SetEditorCurve(clip, newBinding, curve);
        }

        Debug.Log("Se agregó Armature/ correctamente");
    }
}