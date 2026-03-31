# Gera trecho YAML para Mapa_1.unity (grupos + botoes goblin/elfo/anao)
TMP_TAIL = r"""  m_fontColor: {r: 1, g: 1, b: 1, a: 1}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontColorGradient:
    topLeft: {r: 1, g: 1, b: 1, a: 1}
    topRight: {r: 1, g: 1, b: 1, a: 1}
    bottomLeft: {r: 1, g: 1, b: 1, a: 1}
    bottomRight: {r: 1, g: 1, b: 1, a: 1}
  m_fontColorGradientPreset: {fileID: 0}
  m_spriteAsset: {fileID: 0}
  m_tintAllSprites: 0
  m_StyleSheet: {fileID: 0}
  m_TextStyleHashCode: -1183493901
  m_overrideHtmlColors: 0
  m_faceColor:
    serializedVersion: 2
    rgba: 4294967295
  m_fontSize: 8
  m_fontSizeBase: 8
  m_fontWeight: 400
  m_enableAutoSizing: 0
  m_fontSizeMin: 0
  m_fontSizeMax: 0
  m_fontStyle: 0
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 65535
  m_characterSpacing: 0
  m_characterHorizontalScale: 1
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_TextWrappingMode: 1
  m_wordWrappingRatios: 0.4
  m_overflowMode: 0
  m_linkedTextComponent: {fileID: 0}
  parentLinkedComponent: {fileID: 0}
  m_enableKerning: 0
  m_ActiveFontFeatures: 00000000
  m_enableExtraPadding: 0
  checkPaddingRequired: 0
  m_isRichText: 1
  m_EmojiFallbackSupport: 1
  m_parseCtrlCharacters: 1
  m_isOrthographic: 1
  m_isCullingEnabled: 0
  m_horizontalMapping: 0
  m_verticalMapping: 0
  m_uvLineOffset: 0
  m_geometrySortingOrder: 0
  m_IsTextObjectScaleStatic: 0
  m_VertexBufferAutoSizeReduction: 0
  m_useMaxVisibleDescender: 1
  m_pageToDisplay: 1
  m_margin: {x: 0, y: 0, z: 0, w: 0}
  m_isUsingLegacyAnimationComponent: 0
  m_isVolumetricText: 0
  m_hasFontAssetChanged: 0
  m_baseMaterial: {fileID: 0}
  m_maskOffset: {x: 0, y: 0, z: 0, w: 0}"""


def grupo(go_id, rect_id, name, father, children_rects):
    ch = "\n".join(f"  - {{fileID: {c}}}" for c in children_rects)
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {rect_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{rect_id}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
{ch}
  m_Father: {{fileID: {father}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
"""


def main_btn(base_go, rect, cr, img, btn, name, father):
    return f"""--- !u!1 &{base_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {rect}}}
  - component: {{fileID: {cr}}}
  - component: {{fileID: {img}}}
  - component: {{fileID: {btn}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{rect}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {base_go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {father}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 1}}
  m_AnchorMax: {{x: 0, y: 1}}
  m_AnchoredPosition: {{x: 641, y: -270}}
  m_SizeDelta: {{x: 100, y: 100}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {base_go}}}
  m_CullTransparentMesh: 1
--- !u!114 &{img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {base_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 5908448910823415357, guid: e24a3bef4cca7a54791b8be6dd82e7ef, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{btn}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {base_go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {img}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
"""


def unit_btn_mineiro(go_id, rect_id, cr, img, btn, scr, tgo, tr, tcr, tmp, name, father, sprite_fid, sprite_guid, custo, pref_fid, pref_guid, nome_inst):
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {rect_id}}}
  - component: {{fileID: {cr}}}
  - component: {{fileID: {img}}}
  - component: {{fileID: {btn}}}
  - component: {{fileID: {scr}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{rect_id}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {tr}}}
  m_Father: {{fileID: {father}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 1}}
  m_AnchorMax: {{x: 0, y: 1}}
  m_AnchoredPosition: {{x: 640.6, y: -195.09}}
  m_SizeDelta: {{x: 48, y: 48}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_CullTransparentMesh: 1
--- !u!114 &{img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {sprite_fid}, guid: {sprite_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{btn}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {img}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
--- !u!114 &{scr}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4a3b2c1d0e9f8a7b6c5d4e3f2a1b0c9, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::BotaoUnidadesMineiroHumanoController
  custoOuro: {custo}
  mineiroPrefab: {{fileID: {pref_fid}, guid: {pref_guid}, type: 3}}
  nomeInstancia: {nome_inst}
  textoCusto: {{fileID: {tmp}}}
  botao: {{fileID: {btn}}}
--- !u!1 &{tgo}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr}}}
  - component: {{fileID: {tcr}}}
  - component: {{fileID: {tmp}}}
  m_Layer: 0
  m_Name: TextoCusto
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{tr}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {tgo}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {rect_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 0}}
  m_AnchoredPosition: {{x: 0, y: 16.6}}
  m_SizeDelta: {{x: 0, y: 14}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{tcr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {tgo}}}
  m_CullTransparentMesh: 1
--- !u!114 &{tmp}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {tgo}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Unity.TextMeshPro::TMPro.TextMeshProUGUI
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 0.84, b: 0, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: '{custo}'
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: 8f586378b4e144a9851e7b34d9b748ee, type: 2}}
  m_sharedMaterial: {{fileID: 2100000, guid: 79459efec17a4d00a321bdcc27bbc385, type: 2}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294902015
{TMP_TAIL}
"""


def unit_btn_linha(go_id, rect_id, cr, img, btn, scr, tgo, tr, tcr, tmp, name, father, ypos, text_y, sprite_fid, sprite_guid, custo, pref_fid, pref_guid, nome_inst, ref_fid, ref_guid, direcao_x, direcao_y):
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {rect_id}}}
  - component: {{fileID: {cr}}}
  - component: {{fileID: {img}}}
  - component: {{fileID: {btn}}}
  - component: {{fileID: {scr}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{rect_id}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {tr}}}
  m_Father: {{fileID: {father}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 1}}
  m_AnchorMax: {{x: 0, y: 1}}
  m_AnchoredPosition: {{x: 640.6, y: {ypos}}}
  m_SizeDelta: {{x: 48, y: 48}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_CullTransparentMesh: 1
--- !u!114 &{img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: {sprite_fid}, guid: {sprite_guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{btn}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {img}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
--- !u!114 &{scr}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 8e9d0c1a2b3c4d5e6f7a8b9c0d1e2f3a, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::BotaoUnidadesSpawnLinhaController
  custoOuro: {custo}
  unidadePrefab: {{fileID: {pref_fid}, guid: {pref_guid}, type: 3}}
  nomeInstancia: {nome_inst}
  offsetPorUnidade: 1.5
  direcaoDeslocamento: {{x: {direcao_x}, y: {direcao_y}}}
  prefabReferenciaContagem: {{fileID: {ref_fid}, guid: {ref_guid}, type: 3}}
  textoCusto: {{fileID: {tmp}}}
  botao: {{fileID: {btn}}}
--- !u!1 &{tgo}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr}}}
  - component: {{fileID: {tcr}}}
  - component: {{fileID: {tmp}}}
  m_Layer: 0
  m_Name: TextoCusto
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{tr}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {tgo}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {rect_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 0}}
  m_AnchoredPosition: {{x: 0, y: {text_y}}}
  m_SizeDelta: {{x: 0, y: 14}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{tcr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {tgo}}}
  m_CullTransparentMesh: 1
--- !u!114 &{tmp}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {tgo}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: f4688fdb7df04437aeb418b961361dc5, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Unity.TextMeshPro::TMPro.TextMeshProUGUI
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 0.84, b: 0, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: '{custo}'
  m_isRightToLeft: 0
  m_fontAsset: {{fileID: 11400000, guid: 8f586378b4e144a9851e7b34d9b748ee, type: 2}}
  m_sharedMaterial: {{fileID: 2100000, guid: 79459efec17a4d00a321bdcc27bbc385, type: 2}}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: 4294902015
{TMP_TAIL}
"""


# Prefab roots + guids
G_GOBLIN_G = ("180006139801302925", "4dce034506e8af14d90806486d083fd8")
G_GOBLIN_L = ("2617070310612995912", "690773445d65fe440b6246a20ea19f3b")
G_ELFO_A = ("2617070310612995912", "67ffe26d5da7fed448647a9ec7a3ad0d")
G_ELFO_M = ("6527514975530341288", "189dccab99fd98d45a0be33ac8193279")
G_ANAO = ("2617070310612995912", "7316eb756c2ae2f4fa7b71f11528b6cf")
G_GOBLIN_MIN = ("8200200200200200", "a1b2c3d4e5f678901234567890abcd02")
G_ELFO_MIN = ("8100100100100100", "a1b2c3d4e5f678901234567890abcd01")
G_ANAO_MIN = ("8300300300300300", "a1b2c3d4e5f678901234567890abcd03")

# Sprites (fileID, guid)
S_G_M = ("5295933765287912211", "b1c3c35701679474481e0891b1c86217")
S_G_A = ("-6829909036314171477", "0cce1130777deec4eaa908f2c8c6ea9d")
S_G_F = ("8510445725535147641", "7c718c43b00181b4391569d2265cc424")
S_E_M = ("3252559166015548767", "e69be1ea68c430748b4426f462bd6676")
S_E_A = ("-3445640073724540430", "972cbcd85da2dde45b6ca1c08d5abd7b")
S_E_F = ("-1675007256199167650", "4204139f191ab1b4eaadf33023ccea86")
S_A_M = ("1575381922736501885", "b4b1878f94f623a4a8f2d66f6fc251e9")
S_A_A = ("6939494916429292129", "22848a86afdfc6a4eabe8471cbeab685")

if __name__ == "__main__":
    parts = []
    # grupos
    parts.append(grupo(888440000, 888440001, "GrupoUnidadesHumano", 509434001, [509434010, 509434031, 509434041]))
    parts.append(grupo(888440010, 888440011, "GrupoUnidadesGoblin", 509434001, [888460101, 888460111, 888460121]))
    parts.append(grupo(888440020, 888440021, "GrupoUnidadesElfo", 509434001, [888460201, 888460211, 888460221]))
    parts.append(grupo(888440030, 888440031, "GrupoUnidadesAnao", 509434001, [888460301, 888460311, 888460321]))

    # main racial buttons
    parts.append(main_btn(888450000, 888450001, 888450002, 888450003, 888450004, "BotaoUnidadesGoblin", 1967575614))
    parts.append(main_btn(888451000, 888451001, 888451002, 888451003, 888451004, "BotaoUnidadesElfo", 1967575614))
    parts.append(main_btn(888452000, 888452001, 888452002, 888452003, 888452004, "BotaoUnidadesAnao", 1967575614))

    # Goblin units
    parts.append(unit_btn_mineiro(888460100, 888460101, 888460102, 888460103, 888460104, 888460105, 888460106, 888460107, 888460108, 888460109,
        "BotaoUnidadesMineiroGoblin", 888440011, S_G_M[0], S_G_M[1], 70, G_GOBLIN_MIN[0], G_GOBLIN_MIN[1], "GoblinMineiro"))
    parts.append(unit_btn_linha(888460110, 888460111, 888460112, 888460113, 888460114, 888460115, 888460116, 888460117, 888460118, 888460119,
        "BotaoUnidadesArqueiroGoblin", 888440011, -149.52, 14.63, S_G_A[0], S_G_A[1], 100, G_GOBLIN_L[0], G_GOBLIN_L[1], "GoblinLanceiro",
        G_GOBLIN_L[0], G_GOBLIN_L[1], -1, 0))
    parts.append(unit_btn_linha(888460120, 888460121, 888460122, 888460123, 888460124, 888460125, 888460126, 888460127, 888460128, 888460129,
        "BotaoUnidadesFeiticeiroGoblin", 888440011, -104.1, 14.63, S_G_F[0], S_G_F[1], 110, G_GOBLIN_G[0], G_GOBLIN_G[1], "GoblinFeiticeiro",
        G_GOBLIN_G[0], G_GOBLIN_G[1], 1, 0))

    # Elfo
    parts.append(unit_btn_mineiro(888460200, 888460201, 888460202, 888460203, 888460204, 888460205, 888460206, 888460207, 888460208, 888460209,
        "BotaoUnidadesMineiroElfo", 888440021, S_E_M[0], S_E_M[1], 70, G_ELFO_MIN[0], G_ELFO_MIN[1], "ElfoMineiro"))
    parts.append(unit_btn_linha(888460210, 888460211, 888460212, 888460213, 888460214, 888460215, 888460216, 888460217, 888460218, 888460219,
        "BotaoUnidadesArqueiroElfo", 888440021, -149.52, 14.63, S_E_A[0], S_E_A[1], 100, G_ELFO_A[0], G_ELFO_A[1], "ElfoArqueiro",
        G_ELFO_A[0], G_ELFO_A[1], -1, 0))
    parts.append(unit_btn_linha(888460220, 888460221, 888460222, 888460223, 888460224, 888460225, 888460226, 888460227, 888460228, 888460229,
        "BotaoUnidadesFeiticeiroElfo", 888440021, -104.1, 14.63, S_E_F[0], S_E_F[1], 110, G_ELFO_M[0], G_ELFO_M[1], "ElfoMago",
        G_ELFO_M[0], G_ELFO_M[1], 1, 0))

    # Anao - read feiticeiro meta guid
    import pathlib
    p = pathlib.Path(r"e:\github\td\TD\Assets\Sprites\HUD\BotaoUnidadesFeiticeiraAnao.png.meta")
    text = p.read_text(encoding="utf-8")
    import re
    m = re.search(r"213: (-?\d+)", text)
    fid = m.group(1) if m else "5651406923842807619"
    mg = re.search(r"guid: ([a-f0-9]{32})", text)
    guid_anao_f = mg.group(1) if mg else ""

    parts.append(unit_btn_mineiro(888460300, 888460301, 888460302, 888460303, 888460304, 888460305, 888460306, 888460307, 888460308, 888460309,
        "BotaoUnidadesMineiroAnao", 888440031, S_A_M[0], S_A_M[1], 70, G_ANAO_MIN[0], G_ANAO_MIN[1], "AnaoMineiro"))
    parts.append(unit_btn_linha(888460310, 888460311, 888460312, 888460313, 888460314, 888460315, 888460316, 888460317, 888460318, 888460319,
        "BotaoUnidadesArqueiroAnao", 888440031, -149.52, 14.63, S_A_A[0], S_A_A[1], 100, G_ANAO[0], G_ANAO[1], "AnaoArqueiro",
        G_ANAO[0], G_ANAO[1], -1, 0))
    parts.append(unit_btn_linha(888460320, 888460321, 888460322, 888460323, 888460324, 888460325, 888460326, 888460327, 888460328, 888460329,
        "BotaoUnidadesFeiticeiroAnao", 888440031, -104.1, 14.63, fid, guid_anao_f, 110, G_ANAO[0], G_ANAO[1], "AnaoFeiticeiro",
        G_ANAO[0], G_ANAO[1], 1, 0))

    out = "\n".join(parts)
    pathlib.Path(r"e:\github\td\TD\tools\_unidades_insert.yaml").write_text(out, encoding="utf-8")
    print("Wrote", len(out), "chars")
