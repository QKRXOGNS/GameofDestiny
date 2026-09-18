// ============================================================================
// AnimationLoader.cs
// 폴더에서 스프라이트를 자동으로 로드하여 SpriteAnimator에 할당
// Resources.Load() 사용 - 런타임 지원
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

public class AnimationLoader : MonoBehaviour
{
    // ========================================================================
    // 설정
    // ========================================================================
    [Header("Sprite Source")]
    [Tooltip("Resources 폴더 기준 경로 (예: 'Sprites/Player' 또는 'Player')")]
    [SerializeField] private string spriteFolder = "Sprites/Player";
    
    [Header("Animation Definitions")]
    [Tooltip("파일명 형식: {name}_{number}.png (예: ready_1.png, attack1_3.png)")]
    [SerializeField] private AnimationDefinition[] animations;
    
    // ========================================================================
    // 정의
    // ========================================================================
    [System.Serializable]
    public class AnimationDefinition
    {
        public string name = "idle";
        public int frameCount = 6;
        public float frameTime = 0.05f;  // 20fps로 설정 (잔상 방지)
        public bool loop = true;
    }
    
    // ========================================================================
    // 컴포넌트
    // ========================================================================
    private SpriteAnimator spriteAnimator;
    
    // ========================================================================
    // 초기화
    // ========================================================================
    private void Awake()
    {
        spriteAnimator = GetComponent<SpriteAnimator>();
    }
    
    private void Start()
    {
        LoadAndSetup();
    }
    
    // ========================================================================
    // 스프라이트 로드 및 설정
    // ========================================================================
    public void LoadAndSetup()
    {
        if (spriteAnimator == null)
        {
            spriteAnimator = GetComponent<SpriteAnimator>();
        }
        
        if (spriteAnimator == null)
        {
            Debug.LogError("[AnimationLoader] SpriteAnimator 컴포넌트가 없습니다!");
            return;
        }
        
        if (animations == null || animations.Length == 0)
        {
            Debug.LogWarning("[AnimationLoader] 애니메이션 정의가 없습니다!");
            return;
        }
        
        // 애니메이션 시퀀스 생성
        List<SpriteAnimator.AnimationSequence> sequences = new List<SpriteAnimator.AnimationSequence>();
        
        foreach (var def in animations)
        {
            Sprite[] frames = LoadSprites(def.name, def.frameCount);
            
            if (frames != null && frames.Length > 0 && frames[0] != null)
            {
                SpriteAnimator.AnimationSequence seq = new SpriteAnimator.AnimationSequence
                {
                    name = def.name,
                    frames = frames,
                    frameTime = def.frameTime,
                    loop = def.loop
                };
                sequences.Add(seq);
                Debug.Log($"[AnimationLoader] 로드됨: {def.name} ({frames.Length} frames)");
            }
            else
            {
                Debug.LogWarning($"[AnimationLoader] 스프라이트 로드 실패: {def.name} (Resources/{spriteFolder}/{def.name}_X.png 확인)");
            }
        }
        
        // SpriteAnimator에 할당
        if (sequences.Count > 0)
        {
            spriteAnimator.SetSequences(sequences);
            Debug.Log($"[AnimationLoader] 완료: {sequences.Count}개 애니메이션");
        }
    }
    
    // ========================================================================
    // 스프라이트 로드 (Resources.Load - 런타임 지원)
    // ========================================================================
    private Sprite[] LoadSprites(string baseName, int frameCount)
    {
        Sprite[] sprites = new Sprite[frameCount];
        int loadedCount = 0;
        
        for (int i = 0; i < frameCount; i++)
        {
            // Resources.Load()로 스프라이트 로드
            // 경로 형식: folder/name_number
            string spriteName = baseName + "_" + (i + 1);
            Sprite sprite = Resources.Load<Sprite>(spriteFolder + "/" + spriteName);
            
            // 대안: 확장자 없이 시도
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(spriteFolder + "/" + baseName + "_" + (i + 1).ToString("D2"));
            }
            
            // 또 다른 대안: 확장자 포함
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(spriteFolder + "/" + baseName + "_" + (i + 1) + ".png");
            }
            
            sprites[i] = sprite;
            if (sprite != null) loadedCount++;
        }
        
        // 하나도 로드 안되면 null 반환
        if (loadedCount == 0)
        {
            Debug.LogWarning($"[AnimationLoader] '{spriteFolder}/{baseName}_X' 스프라이트를 찾을 수 없습니다.");
            return null;
        }
        
        return sprites;
    }
    
    // ========================================================================
    // 에디터용: Resources 폴더로 스프라이트 복사
    // ========================================================================
    #if UNITY_EDITOR
    [ContextMenu("Copy Sprites to Resources")]
    public void CopySpritesToResources()
    {
        UnityEditor.AssetDatabase.StartAssetEditing();
        
        // Resources/Sprites 폴더 생성
        string resourcesPath = "Assets/Resources";
        if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        string spriteResourcesPath = resourcesPath + "/" + spriteFolder;
        string[] pathParts = spriteFolder.Split('/');
        string currentPath = resourcesPath;
        
        foreach (var part in pathParts)
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder(currentPath + "/" + part))
            {
                UnityEditor.AssetDatabase.CreateFolder(currentPath, part);
            }
            currentPath = currentPath + "/" + part;
        }
        
        // 원본 폴더에서 스프라이트 복사
        string sourceFolder = "Assets/px/sprites/skeleton_king/ice";
        if (UnityEditor.AssetDatabase.IsValidFolder(sourceFolder))
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { sourceFolder });
            
            foreach (var guid in guids)
            {
                string sourcePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileName(sourcePath);
                string destPath = spriteResourcesPath + "/" + fileName;
                
                // 이미 존재하면 스킵
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(destPath) != null)
                    continue;
                
                UnityEditor.AssetDatabase.CopyAsset(sourcePath, destPath);
            }
            
            UnityEditor.AssetDatabase.StopAssetEditing();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"[AnimationLoader] 스프라이트를 '{spriteResourcesPath}'로 복사했습니다!");
        }
        else
        {
            Debug.LogError($"[AnimationLoader] 소스 폴더를 찾을 수 없습니다: {sourceFolder}");
        }
    }
    #endif
}
