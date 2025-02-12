using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiplayersBuildAndRun
{
    [MenuItem("Tools/Run MultiPlayer/2 Players")]
    static void PerformWin64Build2Player()
    {
        PerformWin64Build(2);
    }
    [MenuItem("Tools/Run MultiPlayer/3 Players")]
    static void PerformWin64Build3Player()
    {
        PerformWin64Build(3);
    }
    [MenuItem("Tools/Run MultiPlayer/4 Players")]
    static void PerformWin64Build4Player()
    {
        PerformWin64Build(4);
    }
    static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

        for(int i = 1; i <= playerCount; ++i)
        {
            BuildPipeline.BuildPlayer(GetScenesPaths(), "TestBuild/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe", BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);

        }
    }

    static string GetProjectName()
    {
        //어플리케이션 데이터 패쓰에서 상위두단계 파일 패쓰네임이 프로젝트네임이된다.
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string[] GetScenesPaths()//빌드할때 포함시키는 씬(경로명)을 모두 가져온다.
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        return scenes;
    }
}
