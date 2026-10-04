using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace GameOfDestiny.Editor
{
    /// <summary>
    /// 스프라이트 시퀀스를 기반으로 애니메이션 클립을 자동 생성하는 에디터 툴
    /// Tools > Auto Animation Creator 메뉴로 실행
    /// </summary>
    public class AutoAnimationCreator : EditorWindow
    {
        // 애니메이션 저장 기본 경로
        private const string OutputBasePath = "Assets/Animations";
        
        // 대상 폴더
        private string sourceFolder = "Assets/px/sprites/Skeleton Warrior/skeleton axe/axe_hellfire";
        
        // 패턴: word_number.png 형식
        private const string FilePattern = @"^(.+?)_(\d+)$";

        [MenuItem("Tools/Auto Animation Creator")]
        public static void ShowWindow()
        {
            GetWindow<AutoAnimationCreator>("Animation Creator");
        }

        /// <summary>
        /// MenuItem: 애니메이션 자동 생성 실행
        /// </summary>
        [MenuItem("Tools/Auto Animation Creator - Run")]
        public static void CreateAnimations()
        {
            var window = GetWindow<AutoAnimationCreator>();
            CreateAnimationsForFolder(window.sourceFolder);
        }

        /// <summary>
        /// 폴더 내 스프라이트를 분석하여 애니메이션 생성
        /// </summary>
        private static void CreateAnimationsForFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("[AutoAnimationCreator] 소스 폴더가 지정되지 않았습니다.");
                return;
            }

            // 폴더에서 스프라이트 검색
            string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[AutoAnimationCreator] 폴더에서 텍스처를 찾을 수 없습니다: {folderPath}");
                return;
            }

            // 파일명 패턴 분석 및 그룹화
            var animationGroups = new Dictionary<string, List<SpriteInfo>>();
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                
                // 패턴 매칭: word_number
                var match = Regex.Match(fileName, FilePattern);
                if (match.Success)
                {
                    string animationName = match.Groups[1].Value;
                    int frameNumber = int.Parse(match.Groups[2].Value);
                    
                    // 스프라이트 로드
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        if (!animationGroups.ContainsKey(animationName))
                        {
                            animationGroups[animationName] = new List<SpriteInfo>();
                        }
                        animationGroups[animationName].Add(new SpriteInfo
                        {
                            Sprite = sprite,
                            FrameNumber = frameNumber,
                            Path = assetPath
                        });
                    }
                }
            }

            if (animationGroups.Count == 0)
            {
                Debug.LogWarning($"[AutoAnimationCreator] 애니메이션 가능한 패턴(wrod_number)을 찾을 수 없습니다: {folderPath}");
                return;
            }

            // 출력 폴더 생성
            string folderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string outputFolder = $"{OutputBasePath}/{folderName}";
            
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                AssetDatabase.CreateFolder(OutputBasePath, folderName);
            }

            // 애니메이션 클립 생성
            int createdCount = 0;
            foreach (var group in animationGroups.OrderBy(g => g.Key))
            {
                string animName = group.Key;
                var sprites = group.Value.OrderBy(s => s.FrameNumber).ToList();
                
                if (sprites.Count < 2)
                {
                    Debug.LogWarning($"[AutoAnimationCreator] '{animName}' 스프라이트가 부족합니다 (최소 2개 필요).");
                    continue;
                }

                // 애니메이션 클립 생성
                string clipPath = $"{outputFolder}/{animName}.anim";
                
                // 기존 애셋 확인
                var existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                AnimationClip clip;
                
                if (existingClip != null)
                {
                    // 기존 클립 삭제
                    AssetDatabase.DeleteAsset(clipPath);
                    AssetDatabase.SaveAssets();
                }

                // 새 클립 생성
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);

                // 스프라이트 시퀀스 설정
                SetSpriteSequence(clip, sprites);

                // 클립 설정 (Loop Time: true)
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                EditorUtility.SetDirty(clip);
                createdCount++;
                Debug.Log($"[AutoAnimationCreator] 애니메이션 생성: {animName}.anim ({sprites.Count} frames)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AutoAnimationCreator] 완료! {createdCount}개 애니메이션 생성: {outputFolder}");
        }

        /// <summary>
        /// 애니메이션 클립에 스프라이트 시퀀스 설정
        /// </summary>
        private static void SetSpriteSequence(AnimationClip clip, List<SpriteInfo> sprites)
        {
            // 스프라이트 시퀀스 설정
            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            // Object Reference Curve 설정
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i,
                    value = sprites[i].Sprite
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
        }

        // 스프라이트 정보 구조체
        private struct SpriteInfo
        {
            public Sprite Sprite;
            public int FrameNumber;
            public string Path;
        }

        // GUI 표시
        private void OnGUI()
        {
            EditorGUILayout.LabelField("스프라이트 애니메이션 자동 생성", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 소스 폴더 선택
            EditorGUILayout.LabelField("소스 폴더 (스프라이트):");
            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField(sourceFolder);
            if (GUILayout.Button("찾아보기", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("소스 폴더 선택", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        sourceFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        sourceFolder = selectedPath;
                        Debug.LogWarning("[AutoAnimationCreator] Assets 폴더 외부 경로가 선택되었습니다.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 출력 경로 표시
            EditorGUILayout.LabelField("출력 경로:");
            string folderName = string.IsNullOrEmpty(sourceFolder) ? "" : 
                Path.GetFileName(sourceFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string outputPath = string.IsNullOrEmpty(folderName) ? OutputBasePath : $"{OutputBasePath}/{folderName}";
            EditorGUILayout.LabelField(outputPath, EditorStyles.miniLabel);

            EditorGUILayout.Space(10);

            // 파일 패턴 설명
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("파일명 패턴:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("word_number.png");
            EditorGUILayout.LabelField("예: walk_1.png, walk_2.png → walk.anim");
            EditorGUILayout.LabelField("예: attack_1.png, attack_2.png → attack.anim");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // 생성 버튼
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("애니메이션 생성", GUILayout.Height(40)))
            {
                CreateAnimationsForFolder(sourceFolder);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "소스 폴더의 스프라이트를 분석하여\n" +
                "이름 패턴별로 애니메이션 클립을 생성합니다.\n\n" +
                "• 파일명: word_number.png\n" +
                "• 출력: Assets/Animations/[폴더명]/",
                MessageType.Info);
        }
    }
}
