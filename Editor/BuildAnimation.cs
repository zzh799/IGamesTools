/// <summary>
/// 自动生成序列帧动画
/// </summary>
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

public class BuildAnimation : EditorWindow 
{

    //生成出的Prefab的路径
	private static string PrefabPath = "Assets/Resources/Prefabs/Monster";
	//生成出的AnimationController的路径
    private static string AnimationControllerPath = "Assets/Resources/Anim/Monster";
	//生成出的Animation的路径
    private static string AnimationPath = "Assets/Resources/Anim/Monster";
    //美术给的原始图片路径
    private static string ImagePath = Application.dataPath + "/Resources/images/Monster";

    /// <summary>
    /// 临时存储int[]
    /// </summary>
    private int[] IntArray = new int[] { 0, 1 };
    //帧率
    private static int FrameRate = 30;
    //是否循环
    private static bool IsLoop = false;
    //要循环的动作名字
    private static string AnimaName = " 需要循环的动作名用\",\"间隔.例如:xxx,xxx,xxx";
    //每秒播放图片的个数
    private static int FrameNum = 10; 
    //图片类型
    private static int ImagesTypeInt = 0;
    private static string[] ImagesTypeString = new string[] { "png", "jpg" };
    //是否显示帮助
    private static bool IsHelp = false;
    /// <summary>
    /// 创建、显示窗体
    /// </summary>
    public static void Init()
    {
        BuildAnimation window = (BuildAnimation)EditorWindow.GetWindow(typeof(BuildAnimation), true, "BuildAnimation");
        window.Show();
    }

    /// <summary>
    /// 显示窗体里面的内容
    /// </summary>
    private void OnGUI()
    {
        //帧率
        GUILayout.BeginHorizontal();
        GUILayout.Label("Frame Rate ");
        FrameRate = EditorGUILayout.IntSlider(FrameRate, 1, 60);
        GUILayout.EndHorizontal();
        //每秒播放图片的个数
        GUILayout.BeginHorizontal();
        GUILayout.Label("Frame Num ");
        FrameNum = EditorGUILayout.IntSlider(FrameNum, 1, 60);
        GUILayout.EndHorizontal();
        //是否循环
        IsLoop = EditorGUILayout.Toggle("Loop Time ", IsLoop);
        if (IsLoop)
        {
            AnimaName = EditorGUILayout.TextField("Loop Anima Name:", AnimaName);
        }
        
        //图片类型
        ImagesTypeInt = EditorGUILayout.IntPopup("Images Type", ImagesTypeInt, ImagesTypeString, IntArray);
        //开始生成
        if (GUILayout.Button("Start Build Aniamtion"))
        {
            BuildAniamtions();
        }

        //帮助按钮
        if (GUILayout.Button("Help"))
        {
            if (IsHelp)
            {
                IsHelp = false;
            }
            else
            {
                IsHelp = true;
            }
        }

        if (IsHelp)
        {
            GUILayout.TextArea("自动生成序列帧动画\n---------配置说明---------\n1.将动画帧单图以动作为区分分别放在不同的动作文件夹中,然后将本对象的所有动作文件夹放在同一个动画文件夹中.\n2.将整理好的动画文件夹放在GamesToolsRaw/BuildAnimation文件夹下.\n3.图片的TextureType属性要设置成Sprite(2D and UI)\n4.生成好的资源(Animation,AnimationController,Prefabs)将会放入IGamesToolsResources文件夹中\n---------使用说明--------- \n1.选择BuildAnimation文件夹下要生成Aniamtion的动画文件夹，或者不选择任何文件夹默认将BuildAnimation文件夹下的动画全部生成Aniamtion.\n2.单击IGamesTools/BuildAnimation，\n3.打开窗口后选择对应参数，\n4.点击Start Build Aniamtion，\n5.稍等片刻，--批量生成完成。\n---------面板属性说明---------\nFrame Rate:动画帧率\nFrame Num :每秒播放图片的个数\nLoop Time:是否循环,勾选后才会出现Loop Anima Name选项\nLoop Anima Name:需要循环的动画名用\",\"间隔,如果不填写默认为所有动画都是循环\nImages Type:源图片的文件格式,提供PNG和JPG两种\nStart Build Aniamtion: 开始生成按钮\nHelp:帮助按钮");
        }
        
    }

	//生成所有动画
    public void BuildAniamtions() 
	{
        if (!System.IO.Directory.Exists(ImagePath))
        {
            Debug.LogError("请创建Assets/IGamesToolsRaw/BuildAnimation文件夹,并按照格式放入动画资源!!!");
            return;
        }

        //如果没有选中就生成所有
        Object[] defaultAssetArr = Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets);
        if (defaultAssetArr.Length == 0)
        {
            DirectoryInfo raw = new DirectoryInfo(ImagePath);
            foreach (DirectoryInfo dictorys in raw.GetDirectories())
            {
                BuildCompleteAnimation(dictorys);
                
            }	
        }
        else
        {
            foreach (DefaultAsset defaultAsset in defaultAssetArr)
            {
                if (!System.IO.Directory.Exists(ImagePath + "/" + defaultAsset.name))
                {
                    Debug.LogWarning("选中的文件夹" + defaultAsset.name + "不在Assets/IGamesToolsRaw/BuildAnimation文件夹中,请按照规则选择");
                    continue;
                }
                DirectoryInfo dictorys = new DirectoryInfo(ImagePath + "/" + defaultAsset.name);
                BuildCompleteAnimation(dictorys);
            }
        }

        Debug.Log("Build Aniamtion End");
       
	}
    //生成一套完整动画
    static void BuildCompleteAnimation(DirectoryInfo dictorys)
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (DirectoryInfo dictoryAnimations in dictorys.GetDirectories())
        {
            //每个文件夹就是一组帧动画，这里把每个文件夹下的所有图片生成出一个动画文件
            clips.Add(BuildAnimationClip(dictoryAnimations));
        }
        //把所有的动画文件生成在一个AnimationController里
        UnityEditor.Animations.AnimatorController controller = BuildAnimationController(clips, dictorys.Name);
        //最后生成程序用的Prefab文件
        BuildPrefab(dictorys, controller);
    }
    //把文件夹下的所有图片生成出一个动画文件
	static AnimationClip BuildAnimationClip(DirectoryInfo dictorys)
	{
		string animationName = dictorys.Name;
		//查找所有图片，图片后缀为面板选择类型
        FileInfo[] images = dictorys.GetFiles("*." + ImagesTypeString[ImagesTypeInt]);
		AnimationClip clip = new AnimationClip();
		EditorCurveBinding curveBinding = new EditorCurveBinding();
		curveBinding.type = typeof(SpriteRenderer);
		curveBinding.path="";
		curveBinding.propertyName = "m_Sprite";
		ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Length];
		//动画长度是按秒为单位，1/10就表示1秒切10张图片，根据项目的情况可以在面板界面调节
        float frameTime = 1f / FrameNum;
		for(int i =0; i< images.Length; i++){
			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[i].FullName));
			keyFrames[i] =   new ObjectReferenceKeyframe ();
			keyFrames[i].time = frameTime *i;
			keyFrames[i].value = sprite;
		}
		//动画帧率，可在面板界面调节
        clip.frameRate = FrameRate;

        //动画是否循环在面板界面调节
        if (IsLoop)
		{
            if (IsContainAnimaName(animationName))
            {
                SerializedObject serializedClip = new SerializedObject(clip);
                AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
                clipSettings.loopTime = true;
                serializedClip.ApplyModifiedProperties();
            }
		}
		string parentName = System.IO.Directory.GetParent(dictorys.FullName).Name;
		System.IO.Directory.CreateDirectory(AnimationPath +"/"+parentName);
		AnimationUtility.SetObjectReferenceCurve(clip,curveBinding,keyFrames);
		AssetDatabase.CreateAsset(clip,AnimationPath +"/"+parentName +"/" +animationName+".anim");
		AssetDatabase.SaveAssets();
		return clip;
	}

	static UnityEditor.Animations.AnimatorController BuildAnimationController(List<AnimationClip> clips ,string name)
	{
        System.IO.Directory.CreateDirectory(AnimationControllerPath);
		UnityEditor.Animations.AnimatorController animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath +"/"+name+".controller");
        UnityEditor.Animations.AnimatorControllerLayer layer = animatorController.layers[0];
		UnityEditor.Animations.AnimatorStateMachine sm = layer.stateMachine;
		foreach(AnimationClip newClip in clips)
		{
			UnityEditor.Animations.AnimatorState  state = sm.AddState(newClip.name);
            state.motion = newClip;
          
		}
		AssetDatabase.SaveAssets();
		return animatorController;
	}

	static void BuildPrefab(DirectoryInfo dictorys,UnityEditor.Animations.AnimatorController animatorCountorller)
	{
		//生成Prefab 添加一张预览用的Sprite
        FileInfo images = dictorys.GetDirectories()[0].GetFiles("*." + ImagesTypeString[ImagesTypeInt])[0];
		GameObject go = new GameObject();
		go.name = dictorys.Name;
		SpriteRenderer spriteRender =go.AddComponent<SpriteRenderer>();
		spriteRender.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images.FullName));
		Animator animator = go.AddComponent<Animator>();
		animator.runtimeAnimatorController = animatorCountorller;
        System.IO.Directory.CreateDirectory(PrefabPath);
		PrefabUtility.CreatePrefab(PrefabPath+"/"+go.name+".prefab",go);
		DestroyImmediate(go);
	}


	public static string DataPathToAssetPath(string path)
	{
		if (Application.platform == RuntimePlatform.WindowsEditor)
			return path.Substring(path.IndexOf("Assets\\"));
		else
			return path.Substring(path.IndexOf("Assets/"));
	}

    public static bool IsContainAnimaName(string name)
    {
        bool isContain = false;
        //未填写指定循环动画则全部设置为循环
        if (AnimaName[0] == ' ' || AnimaName == "")
        {
            isContain = true;
        }
        else
        {
            int offset = 0;
            for (int j = 0; j < AnimaName.Length; )
            {

                int end = AnimaName.IndexOf(",", offset);
                string DataId;
                if (end == -1)
                {
                    DataId = AnimaName.Substring(offset, AnimaName.Length - offset);
                }
                else
                {
                    DataId = AnimaName.Substring(offset, end - offset);
                }

                offset = end + 1;

                if (offset == 0)
                {
                    j = AnimaName.Length;
                }
                if (DataId == name)
                {
                    isContain = true;
                }
            }
        }
        
        return isContain;
    }

	class AnimationClipSettings
	{
		SerializedProperty m_Property;
		
		private SerializedProperty Get (string property) { return m_Property.FindPropertyRelative(property); }
		
		public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }
		
		public float startTime   { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
		public float stopTime	{ get { return Get("m_StopTime").floatValue; }  set { Get("m_StopTime").floatValue = value; } }
		public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
		public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
		public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }
		
		public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
		public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
		public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
		public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
		public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
		public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
		public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
		public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
		public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
		public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
	}

}
