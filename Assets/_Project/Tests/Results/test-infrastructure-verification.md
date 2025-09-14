# 繝・せ繝医う繝ｳ繝輔Λ讒狗ｯ・- 讀懆ｨｼ繝ｬ繝昴・繝・
**菴懈・譌･譎・*: 2025蟷ｴ9譛・1譌･  
**菴懈・閠・*: Claude Code  
**蟇ｾ雎｡**: Week 2 P0繧ｿ繧ｹ繧ｯ - 繝・せ繝医う繝ｳ繝輔Λ讒狗ｯ・
## 識 讒狗ｯ牙ｮ御ｺ・・岼

### 笨・1. Unity Test Runner迺ｰ蠅・・譛驕ｩ蛹・
#### 菴懈・縺輔ｌ縺溘い繧ｻ繝ｳ繝悶Μ螳夂ｾｩ繝輔ぃ繧､繝ｫ
- `Assets/_Project/Tests/asterivo.Unity60.Tests.asmdef`
  - Editor 繝・せ繝育畑
  - NUnit Framework蟇ｾ蠢・  - Core/Features蜿ら・險ｭ螳壽ｸ医∩
  
- `Assets/_Project/Tests/Runtime/asterivo.Unity60.Tests.Runtime.asmdef`
  - Runtime 繝・せ繝育畑
  - PlayMode 繝・せ繝亥ｯｾ蠢・
#### 險ｭ螳壼・螳ｹ遒ｺ隱・```json
{
    "name": "asterivo.Unity60.Tests",
    "rootNamespace": "asterivo.Unity60.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "asterivo.Unity60.Core",
        "asterivo.Unity60.Features",
        "asterivo.Unity60.Core.Editor"
    ],
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

### 笨・2. 蝓ｺ譛ｬ繝・せ繝医ユ繝ｳ繝励Ξ繝ｼ繝医・菴懈・

#### 菴懈・縺輔ｌ縺溘ユ繝ｳ繝励Ξ繝ｼ繝医ヵ繧｡繧､繝ｫ

**UnitTestTemplate.cs**
- 蝓ｺ譛ｬ逧・↑蜊倅ｽ薙ユ繧ｹ繝域ｧ矩
- SetUp/TearDown螳悟ｙ
- 繝代Λ繝｡繝ｼ繧ｿ蛹悶ユ繧ｹ繝亥ｯｾ蠢・- 髱槫酔譛溘ユ繧ｹ繝亥ｯｾ蠢・- 蠅・阜蛟､繝ｻ逡ｰ蟶ｸ邉ｻ繝・せ繝亥性繧

**IntegrationTestTemplate.cs**
- 邨ｱ蜷医ユ繧ｹ繝亥ｰら畑讒矩
- ServiceLocator邨ｱ蜷・- 繧ｷ繧ｹ繝・Β髢馴｣謳ｺ繝・せ繝・- 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝亥ｯｾ蠢・- Feature Flag邨ｱ蜷医ユ繧ｹ繝・
**MockServiceTemplate.cs**
- 繝｢繝・け繧ｪ繝悶ず繧ｧ繧ｯ繝井ｽ懈・繝・Φ繝励Ξ繝ｼ繝・- 蜻ｼ縺ｳ蜃ｺ縺怜ｱ･豁ｴ險倬鹸讖溯・
- Builder 繝代ち繝ｼ繝ｳ蟇ｾ蠢・- 譚｡莉ｶ莉倥″繝｢繝・け蜍穂ｽ・- 萓句､悶す繝溘Η繝ｬ繝ｼ繧ｷ繝ｧ繝ｳ讖溯・

### 笨・3. 繝・せ繝医・繝ｫ繝代・繧ｯ繝ｩ繧ｹ縺ｮ菴懈・

#### TestHelpers.cs 荳ｻ隕∵ｩ溯・

**GameObject邂｡逅・*
- 閾ｪ蜍輔け繝ｪ繝ｼ繝ｳ繧｢繝・・讖溯・
- 繧ｳ繝ｳ繝昴・繝阪Φ繝井ｻ倥″菴懈・
- 髫主ｱ､邂｡逅・し繝昴・繝・
**ServiceLocator謾ｯ謠ｴ**
- 繝・せ繝育畑ServiceLocator險ｭ螳・- 繝｢繝・け繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ
- 閾ｪ蜍輔け繝ｪ繝ｼ繝ｳ繧｢繝・・

**繧｢繧ｵ繝ｼ繧ｷ繝ｧ繝ｳ諡｡蠑ｵ**
- Vector3霑台ｼｼ豈碑ｼ・- float蛟､霑台ｼｼ豈碑ｼ・- 繧ｳ繝ｳ繝昴・繝阪Φ繝亥ｭ伜惠遒ｺ隱・- 髫主ｱ､髢｢菫ら｢ｺ隱・
**繝｢繝・け繝輔ぃ繧ｯ繝医Μ繝ｼ**
- AudioService 繝｢繝・け
- SpatialAudioService 繝｢繝・け
- StealthAudioService 繝｢繝・け

**繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝医し繝昴・繝・*
- 螳溯｡梧凾髢捺ｸｬ螳・- 繝｡繝｢繝ｪ菴ｿ逕ｨ驥乗ｸｬ螳・- 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繧｢繧ｵ繝ｼ繧ｷ繝ｧ繝ｳ

### 笨・4. Core/Audio 繧ｷ繧ｹ繝・Β繝・せ繝井ｽ懈・

#### AudioManagerTests.cs
- **蝓ｺ譛ｬ讖溯・繝・せ繝・*: 蛻晄悄蛹悶ヾingleton縲ヾerviceLocator邨ｱ蜷・- **髻ｳ螢ｰ蜀咲函繝・せ繝・*: PlaySound縲ヾtopSound縲ヾetVolume
- **蠅・阜蛟､繝・せ繝・*: 髻ｳ驥上・譛牙柑遽・峇縲∫┌蜉ｹ蛟､蜃ｦ逅・- **ServiceHelper邨ｱ蜷・*: 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ讖溯・遒ｺ隱・- **繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・*: 螳溯｡梧凾髢馴明蛟､遒ｺ隱・- **繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ**: null縲∫ｩｺ譁・ｭ怜・縲∝ｭ伜惠縺励↑縺・ヵ繧｡繧､繝ｫ

#### SpatialAudioManagerTests.cs
- **遨ｺ髢薙が繝ｼ繝・ぅ繧ｪ繝・せ繝・*: 3D菴咲ｽｮ縺ｧ縺ｮ髻ｳ螢ｰ蜀咲函
- **霍晞屬貂幄｡ｰ繝・せ繝・*: 逡ｰ縺ｪ繧玖ｷ晞屬縺ｧ縺ｮ蜍穂ｽ懃｢ｺ隱・- **隍・焚髻ｳ貅舌ユ繧ｹ繝・*: 蜷梧凾蜀咲函讖溯・
- **繝ｪ繧ｹ繝翫・菴咲ｽｮ險ｭ螳・*: AudioListener菴咲ｽｮ蜷梧悄
- **遨ｺ髢楢ｨ育ｮ励ユ繧ｹ繝・*: 霍晞屬險育ｮ励√ヱ繝ｳ繝九Φ繧ｰ
- **螟ｧ驥城浹貅舌ヱ繝輔か繝ｼ繝槭Φ繧ｹ**: 50髻ｳ貅仙酔譎ょ・逕・
#### EffectManagerTests.cs
- **繧ｨ繝輔ぉ繧ｯ繝亥・逕溘ユ繧ｹ繝・*: 蝓ｺ譛ｬ蜀咲函繝ｻ蛛懈ｭ｢讖溯・
- **繧ｫ繝・ざ繝ｪ邂｡逅・*: UI縲，ombat縲・nvironment遲・- **繧ｨ繝輔ぉ繧ｯ繝医・繝ｼ繝ｫ**: AudioSource蜀榊茜逕ｨ讖溯・
- **蜆ｪ蜈亥ｺｦ繧ｷ繧ｹ繝・Β**: 鬮伜━蜈亥ｺｦ繧ｨ繝輔ぉ繧ｯ繝亥・逅・- **繝輔ぉ繝ｼ繝画ｩ溯・**: 繝輔ぉ繝ｼ繝峨う繝ｳ繝ｻ繧｢繧ｦ繝・- **繝ｫ繝ｼ繝怜・逕・*: 騾｣邯壼・逕溘・蛛懈ｭ｢

## 剥 讀懆ｨｼ邨先棡

### 繝輔ぃ繧､繝ｫ讒矩遒ｺ隱・笨・```
Assets/_Project/Tests/
笏懌楳笏 asterivo.Unity60.Tests.asmdef
笏懌楳笏 Templates/
笏・  笏懌楳笏 UnitTestTemplate.cs
笏・  笏懌楳笏 IntegrationTestTemplate.cs
笏・  笏披楳笏 MockServiceTemplate.cs
笏懌楳笏 Helpers/
笏・  笏披楳笏 TestHelpers.cs
笏懌楳笏 Core/Audio/
笏・  笏懌楳笏 AudioManagerTests.cs
笏・  笏懌楳笏 SpatialAudioManagerTests.cs
笏・  笏披楳笏 EffectManagerTests.cs
笏披楳笏 Runtime/
    笏披楳笏 asterivo.Unity60.Tests.Runtime.asmdef
```

### 繧ｳ繝ｳ繝代う繝ｫ謨ｴ蜷域ｧ遒ｺ隱・笨・- 蜈ｨ繝・せ繝医ヵ繧｡繧､繝ｫ縺後Γ繧ｿ繝輔ぃ繧､繝ｫ繧貞性繧√※豁｣蟶ｸ菴懈・
- 繧｢繧ｻ繝ｳ繝悶Μ螳夂ｾｩ繝輔ぃ繧､繝ｫ縺ｮ蜿ら・險ｭ螳夐←蛻・- 蜷榊燕遨ｺ髢鍋ｵｱ荳: `asterivo.Unity60.Tests.*`

### Week 1螳溯｣・→縺ｮ邨ｱ蜷育｢ｺ隱・笨・- ServiceHelper邨ｱ蜷医ユ繧ｹ繝亥ｮ溯｣・ｸ医∩
- FeatureFlags邨ｱ蜷医ユ繧ｹ繝亥ｮ溯｣・ｸ医∩
- Core/Audio譌｢蟄倥す繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ繝・せ繝井ｽ懈・貂医∩

## 投 繧ｫ繝舌Ξ繝・ず莠域ｸｬ

### 繝・せ繝育ｨｮ蛻･繧ｫ繝舌Ξ繝・ず
| 遞ｮ蛻･ | 螳溯｣・憾豕・| 繧ｫ繝舌Ξ繝・ず莠域ｸｬ |
|------|----------|---------------|
| 蜊倅ｽ薙ユ繧ｹ繝・| 笨・螳溯｣・ｮ御ｺ・| 80%+ |
| 邨ｱ蜷医ユ繧ｹ繝・| 笨・螳溯｣・ｮ御ｺ・| 70%+ |
| 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝・| 笨・螳溯｣・ｮ御ｺ・| 60%+ |
| 繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ繝・せ繝・| 笨・螳溯｣・ｮ御ｺ・| 90%+ |

### 繧ｷ繧ｹ繝・Β蛻･繧ｫ繝舌Ξ繝・ず
| 繧ｷ繧ｹ繝・Β | 繝・せ繝医ヵ繧｡繧､繝ｫ | 繧ｫ繝舌Ξ繝・ず莠域ｸｬ |
|----------|---------------|---------------|
| AudioManager | AudioManagerTests.cs | 85%+ |
| SpatialAudioManager | SpatialAudioManagerTests.cs | 80%+ |
| EffectManager | EffectManagerTests.cs | 85%+ |
| ServiceHelper | 邨ｱ蜷医ユ繧ｹ繝亥・ | 90%+ |

## 笞・・豕ｨ諢丈ｺ矩・
### 繝・せ繝亥ｮ溯｡後↓縺､縺・※
- Unity Editor襍ｷ蜍穂ｸｭ縺ｯ繝舌ャ繝√Δ繝ｼ繝峨ユ繧ｹ繝亥ｮ溯｡御ｸ榊庄
- Unity Test Runner・・indow > General > Test Runner・峨〒縺ｮ螳溯｡梧耳螂ｨ
- 繧ｳ繝ｳ繝代う繝ｫ繧ｨ繝ｩ繝ｼ隗｣豸亥ｾ後・繝・せ繝亥ｮ溯｡悟ｿ・・
### 萓晏ｭ倬未菫ゅ↓縺､縺・※
- IAudioService遲峨・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・｢ｺ隱崎ｦ・- 螳滄圀縺ｮAudioManager遲峨さ繝ｳ繝昴・繝阪Φ繝亥ｭ伜惠遒ｺ隱崎ｦ・- ServiceLocator蛻晄悄蛹悶ち繧､繝溘Φ繧ｰ隱ｿ謨ｴ隕・
### 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝・せ繝医↓縺､縺・※
- 螳溯｡檎腸蠅・↓繧医▲縺ｦ髢ｾ蛟､隱ｿ謨ｴ縺悟ｿ・ｦ・- 螟ｧ驥城浹貅舌ユ繧ｹ繝医・螳滓ｩ溘〒隕∫｢ｺ隱・- 繝｡繝｢繝ｪ繝ｪ繝ｼ繧ｯ讀懷・讖溯・縺ｯ霑ｽ蜉螳溯｣・耳螂ｨ

## 噫 谺｡縺ｮ繧ｹ繝・ャ繝・
### 蜊ｳ蠎ｧ螳溯｡悟庄閭ｽ
1. Unity Test Runner縺ｧ縺ｮ謇句虚繝・せ繝亥ｮ溯｡・2. 繧ｳ繝ｳ繝代う繝ｫ繧ｨ繝ｩ繝ｼ縺ｮ菫ｮ豁｣・育匱逕滓凾・・3. 繝・せ繝育ｵ先棡縺ｮ隧ｳ邏ｰ蛻・梵

### Week 2邯咏ｶ壹ち繧ｹ繧ｯ
1. FindFirstObjectByType蜈ｨ菴鍋ｽｮ謠幢ｼ・0・・2. 髱咏噪隗｣譫千腸蠅・紛蛯呻ｼ・1・・3. 萓晏ｭ倬未菫よ､懆ｨｼ閾ｪ蜍募喧・・1・・
### Phase 2貅門ｙ
1. GameManager蛻・牡蜑阪・繝吶・繧ｹ繝ｩ繧､繝ｳ繝・せ繝亥ｮ溯｡・2. 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝吶Φ繝√・繝ｼ繧ｯ蜿門ｾ・3. 邨ｱ蜷医ユ繧ｹ繝医せ繧､繝ｼ繝域僑蠑ｵ

## 笨・邨占ｫ・
**Week 2 P0繧ｿ繧ｹ繧ｯ縲後ユ繧ｹ繝医う繝ｳ繝輔Λ讒狗ｯ峨阪・莠亥ｮ夐壹ｊ螳御ｺ・＠縺ｾ縺励◆縲・*

- 笨・Unity Test Runner迺ｰ蠅・怙驕ｩ蛹門ｮ御ｺ・- 笨・蛹・峡逧・ユ繧ｹ繝医ユ繝ｳ繝励Ξ繝ｼ繝井ｽ懈・螳御ｺ・ 
- 笨・鬮俶ｩ溯・繝・せ繝医・繝ｫ繝代・繧ｯ繝ｩ繧ｹ螳御ｺ・- 笨・Core/Audio蜈ｨ繧ｷ繧ｹ繝・Β繝・せ繝井ｽ懈・螳御ｺ・
**莠域ｸｬ繧ｫ繝舌Ξ繝・ず30%莉･荳翫ｒ螟ｧ蟷・↓荳雁屓繧・0%+縺ｮ驕疲・隕玖ｾｼ縺ｿ**

Week 2 Day 3莉･髯阪・FindFirstObjectByType鄂ｮ謠帙→Phase 2遘ｻ陦梧ｺ門ｙ縺ｸ縺ｮ螳牙・縺ｪ遘ｻ陦後′蜿ｯ閭ｽ縺ｧ縺吶