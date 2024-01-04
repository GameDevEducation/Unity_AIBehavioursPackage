#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class SimpleAICharacter : BaseAI
{
}


#if UNITY_EDITOR
[CustomEditor(typeof(SimpleAICharacter))]
public class SimpleAICharacterEditor : BaseAIEditor
{
}
#endif // UNITY_EDITOR