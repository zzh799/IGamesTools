using UnityEngine;
using System.Collections;
using UnityEditor;
public class IGamesTools : Editor 
{
    /// <summary>
    /// 批量图片资源导入设置
    /// 使用说明： 
    /// 1.选择需要批量设置的贴图，
    /// 2.单击IGamesTools/TextureImportSetting，
    /// 3.打开窗口后选择对应参数，
    /// 4.点击Set Texture ImportSettings，
    /// 5.稍等片刻，--批量设置成功。
    /// </summary>
    [MenuItem("IGamesTools/TextureImportSetting")]
    private static void TextureImportSettingInit()
    {
        TextureImportSetting.Init();
    }
    /// <summary>
    /// 自动生成序列帧动画
    /// 配置说明:
    /// 1.将动画帧单图以动作为区分分别放在不同的动作文件夹中,然后将本对象的所有动作文件夹放在同一个动画文件夹中.
    /// 2.将整理好的动画文件夹放在GamesToolsRaw/BuildAnimation文件夹下.
    /// 3.图片的TextureType属性要设置成Sprite(2D and UI)
    /// 4.生成好的资源(Animation,AnimationController,Prefabs)将会放入IGamesToolsResources文件夹中
    /// 使用说明： 
    /// 1.选择BuildAnimation文件夹下要生成Aniamtion的动画文件夹，或者不选择任何文件夹默认将BuildAnimation文件夹下的动画全部生成Aniamtion.
    /// 2.单击IGamesTools/BuildAnimation，
    /// 3.打开窗口后选择对应参数，
    /// 4.点击Start Build Aniamtion，
    /// 5.稍等片刻，--批量生成完成。
    /// 面板属性说明
    /// Frame Rate:动画帧率
    /// Frame Num :每秒播放图片的个数
    /// Loop Time:是否循环,勾选后才会出现Loop Anima Name选项
    /// Loop Anima Name:需要循环的动画名用","间隔,如果不填写默认为所有动画都是循环
    /// Images Type:源图片的文件格式,提供PNG和JPG两种
    /// Start Build Aniamtion: 开始生成按钮
    /// Help:帮助按钮
    /// </summary>
    [MenuItem("IGamesTools/BuildAnimation")]
    private static void BuildAnimaitonInit()
    {
        BuildAnimation.Init();
    }
}
