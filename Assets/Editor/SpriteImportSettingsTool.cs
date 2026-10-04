using UnityEditor;
using UnityEngine;

namespace GameOfDestiny.Editor
{
    /// <summary>
    /// 스프라이트 임포트 설정을 대상 폴더에 적용하는 에디터 툴
    /// Tools > Sprite Import Settings 메뉴로 실행
    /// </summary>
    public class SpriteImportSettingsTool : EditorWindow
    {
        // 고정 스프라이트 임포트 설정값
        private const TextureImporterType TextureType = TextureImporterType.Sprite;
        private const int SpriteMode = 2; // Multiple
        private const bool SRgbTexture = true;
        private const bool EnableMipMap = false;
        private const FilterMode FilterMode = UnityEngine.FilterMode.Point;
        private const int AnisoLevel = 1;
        private const TextureWrapMode WrapMode = TextureWrapMode.Clamp;
        private const int MaxTextureSize = 2048;
        private const int CompressionQuality = 50;
        private const TextureImporterNPOTScale NpotScale = TextureImporterNPOTScale.None;
        private const bool AlphaIsTransparency = true;
        private const float SpritePixelsToUnits = 100f;
        private static readonly Vector2 SpritePivot = new Vector2(0.5f, 0.5f);
        private const int SpriteExtrude = 1;
        private const SpriteMeshType SpriteMeshType = UnityEngine.SpriteMeshType.FullRect;
        private const bool GenerateFallbackPhysicsShape = true;

        // 대상 폴더 경로
        private string targetFolder = "Assets/px/sprites/Skeleton Warrior/skeleton axe/axe_hellfire";

        [MenuItem("Tools/Sprite Import Settings")]
        public static void ShowWindow()
        {
            GetWindow<SpriteImportSettingsTool>("Sprite Import Settings");
        }

        /// <summary>
        /// MenuItem: 대상 폴더의 모든 스프라이트에 설정 적용
        /// </summary>
        [MenuItem("Tools/Sprite Import Settings - Apply")]
        public static void ApplySettings()
        {
            var window = GetWindow<SpriteImportSettingsTool>();
            ApplyToFolder(window.targetFolder);
        }

        private static void ApplyToFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("[SpriteImportSettingsTool] 대상 폴더가 지정되지 않았습니다.");
                return;
            }

            // 대상 폴더의 모든 텍스처 가져오기
            string[] targetGuids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
            if (targetGuids.Length == 0)
            {
                Debug.LogWarning($"[SpriteImportSettingsTool] 폴더에서 텍스처를 찾을 수 없습니다: {folderPath}");
                return;
            }

            int count = 0;
            foreach (string guid in targetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                ApplySettingsToImporter(importer);
                importer.SaveAndReimport();
                count++;
            }

            Debug.Log($"[SpriteImportSettingsTool] {count}개 파일에 설정을 적용했습니다: {folderPath}");
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 고정 설정값을 TextureImporter에 적용
        /// </summary>
        private static void ApplySettingsToImporter(TextureImporter importer)
        {
            // 기본 속성
            importer.textureType = TextureType;
            importer.mipmapEnabled = EnableMipMap;
            importer.sRGBTexture = SRgbTexture;
            importer.filterMode = FilterMode;
            importer.anisoLevel = AnisoLevel;
            importer.wrapModeU = WrapMode;
            importer.wrapModeV = WrapMode;
            importer.wrapModeW = WrapMode;
            importer.maxTextureSize = MaxTextureSize;
            importer.compressionQuality = CompressionQuality;
            importer.npotScale = NpotScale;
            importer.alphaIsTransparency = AlphaIsTransparency;
            importer.spritePixelsPerUnit = SpritePixelsToUnits;
            importer.spritePivot = SpritePivot;

            // Reflection으로 spriteMode 설정
            SetProperty(importer, "spriteMode", SpriteMode);
            SetProperty(importer, "spriteExtrude", SpriteExtrude);
            SetProperty(importer, "spriteMeshType", (int)SpriteMeshType);
            SetProperty(importer, "spriteGenerateFallbackPhysicsShape", GenerateFallbackPhysicsShape);

            // 플랫폼 설정
            ApplyPlatformSettings(importer);
        }

        // Reflection 기반 속성 설정
        private static void SetProperty(TextureImporter importer, string propertyName, object value)
        {
            var prop = importer.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                prop.SetValue(importer, value);
            }
        }

        /// <summary>
        /// 플랫폼 설정 적용
        /// </summary>
        private static void ApplyPlatformSettings(TextureImporter importer)
        {
            // 기본 설정
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "DefaultTexturePlatform",
                maxTextureSize = MaxTextureSize,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = CompressionQuality,
                overridden = false
            });

            // Standalone
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Standalone",
                maxTextureSize = MaxTextureSize,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = CompressionQuality,
                overridden = false
            });

            // Android
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                maxTextureSize = MaxTextureSize,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = CompressionQuality,
                overridden = false
            });

            // iOS
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "iPhone",
                maxTextureSize = MaxTextureSize,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = CompressionQuality,
                overridden = false
            });
        }

        // GUI 표시
        private void OnGUI()
        {
            EditorGUILayout.LabelField("스프라이트 임포트 설정 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 대상 폴더 선택
            EditorGUILayout.LabelField("대상 폴더:");
            EditorGUILayout.BeginHorizontal();
            targetFolder = EditorGUILayout.TextField(targetFolder);
            if (GUILayout.Button("찾아보기", GUILayout.Width(70)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("대상 폴더 선택", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Assets 상대 경로로 변환
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        targetFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        targetFolder = selectedPath;
                        Debug.LogWarning("[SpriteImportSettingsTool] Assets 폴더 외부 경로가 선택되었습니다.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 고정 설정값 표시
            EditorGUILayout.LabelField("적용할 설정값 (고정):", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Texture Type: {TextureType}");
            EditorGUILayout.LabelField($"Sprite Mode: {SpriteMode}");
            EditorGUILayout.LabelField($"Filter Mode: {FilterMode}");
            EditorGUILayout.LabelField($"Mip Map: {(EnableMipMap ? "Enabled" : "Disabled")}");
            EditorGUILayout.LabelField($"Alpha Is Transparency: {AlphaIsTransparency}");
            EditorGUILayout.LabelField($"Max Texture Size: {MaxTextureSize}");
            EditorGUILayout.LabelField($"Compression: {(TextureImporterCompression)1}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // 적용 버튼
            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("설정 적용", GUILayout.Height(40)))
            {
                ApplyToFolder(targetFolder);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "대상 폴더의 모든 스프라이트에\n" +
                "고정된 스프라이트 임포트 설정을 적용합니다.",
                MessageType.Info);
        }
    }
}
