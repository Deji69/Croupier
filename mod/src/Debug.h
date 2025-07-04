#pragma once
#include <Globals.h>
#include <Glacier/Pins.h>

class Debug {
public:
	static auto ShouldSkipPin(int32 pin) -> bool {
#ifndef _DEBUG
		return true;
#endif
		switch (pin) {
			case 501542: // ZEntity (data: bool)
			case 9579636: // ZEntity (data: void)
			case 36746103: // ZCompositeEntity (data: void)
			case 44660150: // ZCompositeEntity (data: void)
			case 71035488: // ZEntity (data: void)
			case 99587158: // ZCompositeEntity (data: void)
			case 122894096: // ZEntity (data: void)
			case 129546075: // ZMenuSoundControllerEntity, ZSniperSoundControllerEntity, ZScoreSoundControllerEntity, ZMenuSoundEventToPinControllerEntity, etc. (data: void)
			case 148920563: // ZEntity (data: void)
			case 179438730: // ZEntity (data: "L2")
			case 211203475: // ZEntity (data: void)
			case 228081014: // ZContractsGamestateEmitterEntity (data: ZDynamicObject)
			case 229468423: // ZTriggerSwitchEntity (data: void)
			case 230702392: // ZMenuSoundControllerEntity (data: void)
			case 230775901: // ZDynamicObjectEntity (data: ZDynamicObject)
			case 253055846: // ZCompositeEntity (data: float32)
			case 271288499: // ZEntity (data: void) - escort situation?
			case 277563714: // ZEntity (data: int32)
			case 281998697: // ZEntity (data: void)
			case 301040445: // ZEntity (data: void)
			case 301147094: // ZEntity (data: void)
			case 306002251: // ZEntity (data: void)
			case 310671713: // ZEntity (data: void)
			case 322468906: // ZEntity (data: void)
			case 357910907: // ZEntity (data: void)
			case 372880910: // ZEntity (data: void)
			case 374906731: // ZEntity (data: bool)
			case 377503514: // ZEntity (data: float32)
			case 403074860: // ZEntity (data: int32)
			case 403940158: // ZEntity (data: void)
			case 416678229: // ZEntity (data: void)
			case 434807908: // ZDefaultMetricsSenderEntity (data: TArray<SDynamicObjectKeyValuePair>)
				/*
				| Props:
				|  ZString m_metricName
				|  bool m_localOnly
				|  TArray<TEntityRef<IMetricValue>> m_ValueArray
				|  TArray<TEntityRef<IMetricValue>> m_localValueArray
				|  bool m_sendHeroPosition
				|  bool m_sendContractId
				|  bool m_sendContractSessionId
				|  bool m_sendRoomId
				| Interfaces:
				|  ZDefaultMetricsSenderEntity
				|  ZMetricsSenderEntity
				|  IEntity
				|  void
				*/
			case 439057430: // ZEntity (data: void)
			case 447011050: // ZEntity (data: void) - m_rActEntity
			case 461162091: // ZEntity (data: ZEntityRef)
			case 470685528: // ZEntity (data: float32)
			case 486625325: // ZCompositeEntity (data: void)
			case 489993566: // ZEntity (data: void)
			case 500373993: // ZEntity (data: void)
			case 515211791: // ZCompositeEntity (data: void)
			case 521978576: // ZSequenceEntity (data: void)
			case 556922020: // ZMenuSoundEventToPinControllerEntity (data: void)
			case 573986084: // ZEntity (data: bool)
			case 601231859: // ZEntity (data: void)
			case 601896093: // ZEntity (data: void)
			//case 607230171: // ZEntity (data: "L2") - Box volumes, level changes?
			case 635678676: // ZEntity (data: void)
			case 668485891: // ZEntity (data: void)
			case 669530264: // ZEntity (data: int32)
			case 674185058: // ZCompositeEntity (data: void)
			case 683393201: // ZEntity (data: void)
			case 691848172: // ZEntity (data: void)
			case 717138142: // ZEntity (data: void)
			case 741687867: // ZEntity (data: void)
			case 745799826: // ZEntity (data: void)
			case 752606684: // ZEntity (data: void)
			case 754018329: // ZEntity (data: void) - ZEscortSituationEntity, ISituation
				/*
				|  Props:
				|  TEntityRef<ZHM5BaseCharacter> m_rEscortTarget
				|  TArray<TEntityRef<ZActor>> m_aActors
				|  TArray<SVector3> m_aTargetOffsets
				|  TArray<TEntityRef<IBoolCondition>> 1152975217
				|  TArray<TEntityRef<ZScreenplay>> m_aIntermediateScreenplays
				|  int32 m_nPriority
				|  bool m_bIsEnabled
				|  bool m_bIsRunning
				|  float32 m_fMaxDistanceToIntermediates
				|  float32 m_fMaxDistanceToIntermediatesDuringEscort
				|  float32 m_fMaxTargetDistance
				|  bool m_bEscortsReactToDistactions
				|  float32 m_fSpeedUpDistance
				|  float32 m_fTargetPathOffsetMax
				|  float32 m_fTargetPathOffsetMin
				|  TEntityRef<ZNPCAnimationSetDefinition> m_rAnimVariationResource
				|  bool m_bTargetSlowsDownWhenOutOfRange
				|  ZString m_sMatchNode
				|  TEntityRef<ZVIPControllerEntity> m_rVIPController
				|  bool m_bStartSearchOnTargetDead
				|  float32 m_fStartSearchOnTargetDeadDelay
				|  float32 m_fStartSearchOnTargetDeadSubsequentDelay
				|  float32 m_fMaxVerticalDistanceToIntermediates
				|  bool m_bOverrideTetherDistances
				|  float32 m_fOverrideTetherDistanceMin
				|  float32 m_fOverrideTetherDistanceMax
				|  bool m_bSetDefaultAnimVariationsOnLeaveSituation
				|  TEntityRef<IBoolCondition> m_rApplyAnimVariationCondition
				|  float32 m_fWaitForApproachingTargetDistance
				*/
			case 768319972: // ZEntity (data: void)
			case 792251342: // ZEntity (data: void)
			case 806167747: // ZEntity (data: void)
			case 845012947: // ZEntity (data: void)
			case 856531663: // ZEntity (data: bool)
			case 859762941: // ZEntity (data: void)
			case 866574461: // ZCompositeEntity (data: void)
			case 873418195: // ZEntity (data: bool)
			case 877868260: // ZEntity (data: void)
			case 882644225: // ZEntity (data: void)
			case 889090044: // ZEntity (data: void)
			case 889701719: // ZEntity (data: int32)
			case 891260949: // ZEntity (data: SColorRGB)
			case 894903842: // ZCompositeEntity (data: void)
			case 908785572: // ZCompositeEntity (data: float32)
			case 930506055: // ZEntity (data: float32)
			case 935673879: // ZEntity (data: void) -- ZReplicableAspect?
			case 937832496: // ZCompositeEntity (data: void)
			case 937885519: // ZEntity (data: void)
			case 946677172: // ZUIControlEntity (data: int32)
			case 947557433: // ZEntity (data: bool)
			case 950003767: // ZEntity (data: void)
			case 952836605: // ZEntity (data: void)
			case 961221181: // ZEntity (data: void)
			case 963803069: // ZEntity (data: void)
			case 975996388: // ZEntity (data: void)
			case 976488807: // ZEntity (data: void)
			case 1009171624: // ZEntity (data: void)
			case 1006823794: // ZEntity (data: bool)
			case 1023633462: // ZEntity (data: "AmbientZone")
			case 1034502986: // ZEntity (data: void)
			case 1038448847: // ZEntity (data: void)
			case 1039110470: // ZEntity (data: void)
			case 1080022118: // ZEntity (data: bool)
			case 1089554643: // ZEntity (data: bool)
			case 1101737245: // ZEntity (data: void)
			case 1129579634: // ZEntity (data: void)
			case 1131861442: // ZEntity (data: void)
			case 1144635499: // ZEntity (data: void)
			case 1156972293: // ZEntity (data: ZEntityRef)
			case 1163998226: // ZEntity (data: void)
			case 1170839340: // ZEntity (data: void) - ZMenuSoundControllerEntity related
			case 1202154661: // ZEntity (data: "inc +1-WhiteDot")
			case 1204287162: // ZEntity (data: void)
			case 1241202468: // ZValueFloat_Set (data: void)
			case 1253692480: // ZEntity (data: void)
			case 1256343641: // ZEntity (data: void)
			case 1258249942: // ZEntity (data: void)
			case 1286797620: // ZTimeOfDayRangeEntity (data: bool)
			case 1291046594: // ZGeomEntity (data: float32)
			case 1335210015: // ZEntity (data: void)
			case 1347014061: // ZEntity (data: void)
			case 1348494359: // ZUIControlEntity (data: bool)
			case 1357543355: // ZCompositeEntity (data: EGait)
			case 1368605563: // ZEntity (data: void)
			case 1377209749: // ZEntity (data: bool)
			case 1405650200: // ZEntity (data: ZEntityRef)
			case 1413678324: // ZEntity (data: void)
			case 1414783256: // ZEntity (data: void)
			case 1429745763: // ZEntity (data: void)
			case 1434133646: // ZEntity (data: void)
			case 1449711450: // ZGeomEntity (data: bool)
			case 1458155033: // ZEntity (data: void)
			case 1477225221: // ZEventTrack (data: void)
			case 1489696997: // ZEntity (data: void)
			case 1511806660: // ZEntity (data: void)
			case 1513651282: // ZSequenceEntity (data: void)
			case 1516324495: // ZEntity (data: SVector3)
			case 1528949279: // ZEntity (data: void)
			case 1562258235: // ZEntity (data: void)
			case 1574471497: // ZEventTrack (data: void)
			case 1615863646: // ZEntity (data: void)
			case 1617587914: // ZEntity (data: ZEntityRef)
			case 1628839158: // ZEntity (data: bool)
			case 1631403235: // ZEntity (data: void)
			case 1643757213: // ZEntity (data: void)
			case 1644512933: // ZEntity (data: void)
			case 1651641059: // ZEntity (data: void)
			case 1664933465: // ZEntity (data: void)
			case 1666580196: // ZEntity (data: void)
			case 1679099051: // ZEntity (data: void)
			case 1680474940: // ZEntity (data: bool)
			case 1708316975: // ZMenuSoundControllerEntity, ZSniperSoundControllerEntity, ZScoreSoundControllerEntity... (data: void)
			case 1720290305: // ZEntity (data: void)
			case 1727745808: // ZCompositeEntity (data: void)
			case 1737185346: // ZEntity (data: bool)
			case 1754318198: // ZEntity (data: void)
			case 1767189454: // ZGeomEntity (data: float32)
			case 1773487849: // ZEventTrack (data: void)
			case 1778428257: // ZStringToSignalForwarderEntity (data: void)
				/*
				| Props:
				|  ZString m_sSignalName
				| Interfaces:
				|  ZStringToSignalForwarderEntity
				|  IEntity
				|  void
				*/
			case 1789489423: // ZGeomEntity (data: void)
			case 1818736075: // ZEntity (data: void)
			case 1822771111: // ZEntity (data: "inc +1-WhiteDot")
			case 1831366352: // ZEntity (data: void)
			case 1842488873: // ZEntity (data: void)
			case 1846136316: // ZEntity (data: void)
			case 1854117489: // ZSequenceEntity (data: void)
			case 1858673265: // ZCompositeEntity (data: void)
			case 1863619496: // ZEntity (data: void)
			case 1895620862: // ZEntity (data: bool)
			case 1901097519: // ZEntity (data: void)
			case 1903696663: // ZEntity (data: float32)
			case 1921326538: // ZEntity (data: void)
			case 1940498321: // ZEntity (data: "L2")
			case 1940520998: // ZEntity (data: ZEntityRef)
			case 1944367749: // ZEntity (data: void)
			case 1954375505: // ZEntity (data: void)
			case 1956952087: // ZEntity (data: void)
			case 1959624952: // ZEntity (data: void)
			case 1980660862: // ZEntity (data: void)
			case 1990250279: // ZEntity (data: bool)
			case 2008889714: // ZEntity (data: void)
			case 2039108245: // ZCompositeEntity (data: void)
			case 2042091877: // ZEntity (data: ZEntityRef)
			case 2047473119: // ZEntity (data: void)
			case 2055954104: // ZEntity (data: void) "actor go to point?" seems related to dramas/acts
			case 2057984401: // ZTriggerSwitchEntity (data: void)
			case 2076541628: // ZCompositeEntity (data: float32)
			case 2086034889: // ZEntity (data: float32)
			case 2089164703: // ZEntity (data: bool)
			case 2089510909: // ZEntity (data: void)
			case 2098590534: // ZEntity (data: float32)
			case 2100872677: // ZEntity (data: "Dec-1 - NoActivity")
			case 2107543273: // ZEntity (data: void)
			case 2139530562: // ZEntity (data: void)
			case -38235880: // ZEntity (data: void)
			case -55772032: // ZEntity (data: void)
			case -88134018: // ZEntity (data: void)
			case -103069261: // ZEntity (data: void)
			case -106164073: // ZEntity (data: void)
			case -114013006: // ZEntity (data: bool)
			case -114346585: // ZUIControlEntity (data: int32)
			case -119187254: // ZEntity (data: void)
			case -125993919: // ZEntity (data: void)
			case -177663794: // ZEntity (data: void)
			case -183274817: // ZGeomEntity (data: void)
			case -213952696: // ZEntity (data: void)
			case -220964066: // ZEntity (data: void)
			case -227254801: // ZUISubtitleDataProvider (data: bool)
			case -233999739: // ZEntity (data: float32)
			case -237391468: // ZEntity (data: void)
			case -255120762: // ZEntity (data: bool)
			case -290605427: // ZEntity (data: void)
			case -296888379: // ZEntity (data: void)
			case -307063471: // ZEntity (data: void)
			case -318742697: // ZEntity (data: void)
			case -322926365: // ZEntity (data: void)
			case -327208175: // ZEntity (data: void)
			case -351248558: // ZItemSpawner (data: void)
			case -357930714: // ZEntity (data: void)
			case -358295328: // ZEntity (data: void)
			case -390209647: // ZEntity (data: bool)
			case -393803563: // ZEntity (data: float32)
			case -395574818: // ZEntity (data: void)
			case -399027006: // ZEntity (data: void)
			case -402475892: // ZEntity (data: void)
			case -410343194: // ZEntity (data: ZEntityRef)
			case -417854196: // ZEntity (data: void)
			case -418092396: // ZCompositeEntity (data: void)
			case -421224544: // ZEntity (data: void)
			case -423737797: // ZEntity (data: TEntityRef<IActor>)
			case -427559050: // ZEntity (data: void)
			case -432873473: // ZEntity (data: void)
			case -443162547: // ZEntity (data: void)
			case -456209358: // ZTriggerSwitchEntity (data: void)
			case -469755470: // ZMenuSoundEventToPinControllerEntity (data: void)
			case -475854805: // ZTriggerSwitchEntity (data: void)
			case -498354609: // ZEntity (data: void)
			case -495579654: // ZEntity (data: void)
			case -518499245: // ZEntity (data: void)
			case -519995521: // ZEntity (data: bool)
			case -531765867: // ZEntity (data: void)
			case -534381239: // ZEntity (data: void)
			case -584019503: // ZEntity (data: void)
			case -588961975: // ZCompositeEntity (data: float32)
			case -603950521: // ZEntity (data: void)
			case -624852522: // ZEntity (data: void)
			case -631417400: // ZEntity (data: void)
			case -640252619: // ZEntity (data: void)
			case -651205166: // ZEntity (data: void)
			case -708689034: // ZEntity (data: void)
			case -712745208: // ZEntity (data: void)
			case -747304223: // ZEntity (data: void)
			case -754735179: // ZEntity (data: bool)
			case -757798211: // ZEntity (data: void)
			case -766372164: // ZEntity (data: void) - opportunity related?
			case -774731285: // ZEntity (data: void)
			case -782324219: // ZEntity (data: bool)
			case -796675733: // ZCompositeEntity (data: void)
			case -805067403: // ZEntity (data: void)
			case -805990237: // ZUIMapControllerEntity (data: void)
			case -814087883: // ZEntity (data: void)
			case -855313396: // ZEntity (data: void)
			case -861928259: // ZCompositeEntity (data: SColorRGB)
			case -885635335: // ZEntity (data: void)
			case -895397753: // ZEntity (data: bool)
			case -908102272: // ZEventTrack (data: void)
			case -909792276: // ZEntity (data: void) - waypoint stuff possibly
			case -916355607: // ZCompositeEntity (data: float32)
			case -917093329: // ZEntity (data: void)
			case -924185415: // ZMenuSoundEventToPinControllerEntity (data: void)
			case -937580624: // ZEntity (data: void)
			case -944647555: // ZMenuSoundControllerEntity (data: void)
			case -945570440: // ZEntity (data: void)
			case -952574277: // ZEntity (data: ZEntityRef)
			case -955845310: // ZEntity (data: void)
			case -959709332: // ZEntity (data: void)
			case -966059140: // ZEntity (data: ZEntityRef)
			case -972667076: // ZEntity (data: void)
			case -980979663: // ZEntity (data: void)
			case -991045767: // ZEntity (data: void)
			case -1002691499: // ZMenuSoundEventToPinControllerEntity (data: void)
			case -1007933124: // ZEntity (data: void)
			case -1043195759: // ZCompositeEntity (data: void)
			case -1047643544: // ZEntity (data: void)
			case -1094069602: // ZGamePauseEntity (data: void)
			case -1101488211: // ZSequenceEntity (data: void)
			case -1102398408: // ZEntity (data: void)
			case -1116158045: // ZEntity (data: void)
			case -1118215983: // ZEntity (data: void)
			case -1110338595: // ZContractsGamestateEmitterEntity (data: ZRepositoryID)
			case -1111308979: // ZEntity (data: void)
			case -1168358397: // ZEntity (data: void)
			case -1176065269: // ZCompositeEntity (data: float32)
			case -1180291634: // ZEntity (data: void)
			case -1180859626: // ZEntity (data: void)
			case -1183097825: // ZEntity (data: void)
			case -1184002062: // ZEntity (data: void)
			case -1191100333: // ZUIControlEntity (data: void)
			case -1206422364: // ZEntity (data: void)
			case -1206812497: // ZEntity (data: void)
			case -1208951912: // ZEntity (data: void)
			case -1219054791: // ZEntity (data: float32)
			case -1222512624: // ZEntity (data: void)
			case -1224423696: // ZEntity (data: void) - opportunity related?
			case -1229587178: // ZEntity (data: void)
			case -1230876178: // ZEntity (data: void)
			case -1240590791: // ZEntity (data: void)
			case -1249253121: // ZEntity (data: void)
			case -1258157561: // ZEntity (data: ZEntityRef)
			case -1269514813: // ZEntity (data: void)
			case -1297124618: // ZEntity (data: void)
			case -1302741857: // ZEntity (data: void)
			case -1326194532: // ZEntity (data: void)
			case -1329698620: // ZUISubtitleDataProvider (data: void)
			case -1324017218: // ZEntity (data: void)
			case -1333480518: // ZEntity (data: void)
			case -1369714487: // ZEntity (data: void)
			case -1379886238: // ZEntity (data: void)
			case -1383748470: // ZEntity (data: void)
			case -1385943967: // ZLevelControlEntity (data: bool)
			case -1386584738: // ZEntity (data: void)
			case -1386708075: // ZEntity (data: void)
			case -1389845701: // ZEntity (data: void)
			case -1391241443: // ZUIControlEntity (data: void)
			case -1405413752: // ZEntity (data: void)
			case -1405696677: // ZEntity (data: void)
			case -1427322264: // ZEntity (data: void)
			case -1439194809: // ZEntity (data: void)
			case -1441226063: // ZCompositeEntity (data: void)
			case -1454145363: // ZEntity (data: bool)
			case -1465899494: // ZEntity (data: void)
			case -1474825892: // ZEntity (data: void)
			case -1492098990: // ZEntity (data: float32)
			case -1510806797: // ZEntity (data: void)
			case -1519020416: // ZEntity (data: void)
				/*
				| Props:
				|  EGestureCategory m_eGesture
				|  TArray<TEntityRef<IBoolCondition>> 163509863
				|  int32 469683494
				|  ZString m_sDebugAudioEventName
				|  ZResourcePtr 2991212910
				|  TEntityRef<ICharacterSpeakController> 4264415607
				|  bool m_bEnableDistanceCulling
				|  bool 569529742
				*/
			case -1529617423: // ZEntity (data: void)
			case -1537244910: // ZEntity (data: void)
			case -1542831599: // ZSequenceEntity (data: void)
			case -1543545464: // ZEntity (data: void)
			case -1551962697: // ZGeomEntity (data: ZEntityRef)
			case -1567676047: // ZDefaultMetricsSenderEntity (data: ZRepositoryID)
			case -1570316685: // ZEntity (data: void)
			case -1584511657: // ZEntity (data: ZEntityRef)
			case -1603967581: // ZEntity (data: void)
			case -1611503415: // ZEntity (data: void)
			case -1626476704: // ZEntity (data: void)
			case -1629124598: // ZEntity (data: void)
			case -1635003431: // ZEntity (data: SVector3)
			case -1649071104: // ZEntity (data: "L2")
			case -1668320543: // ZEntity (data: void)
			case -1670848567: // ZMenuSoundEventToPinControllerEntity (data: void)
			case -1687807995: // ZEntity (data: void)
			case -1692386444: // ZEntity (data: void)
			case -1727780610: // ZEntity (data: void)
			case -1743318979: // ZEntity (data: void)
			case -1777714596: // ZHM5SequenceSkipEntity (data: void)
			case -1779235007: // ZEntity (data: void)
				/*
				| Props:
				|  TEntityRef<ZRepositoryItemEntity> m_Item
				|  ZString 4262580536
				|  ZString 4262580536
				|  TArray<TEntityRef<IMetricValue>> 2282108046
				|  TArray<TEntityRef<IMetricValue>> m_MetricsValue
				|  bool m_bSendDirectly
				| Interfaces:
				|  ZEntity
				|  IEntity
				*/
			case -1782138463: // ZEntity (data: void)
			case -1788401424: // ZCameraEntity (data: int32)
			case -1789792846: // ZMenuSoundControllerEntity (data: void)
			case -1799300766: // ZEntity (data: void)
			case -1801176899: // ZEntity (data: void)
			case -1807379210: // ZEntity (data: void)
			case -1808189318: // ZEntity (data: bool)
			case -1823528714: // ZUIControlEntity (data: void)
			case -1839449382: // ZEntity (data: int32)
			case -1844322573: // ZUIControlEntity (data: void)
			case -1860897575: // ZEntity (data: void)
			case -1866239072: // ZEntity (data: void)
			case -1868879805: // ZSequenceEntity (data: void)
			case -1874226852: // ZEntity (data: void)
			case -1877658337: // ZEntity (data: SColorRGB)
			case -1888797396: // ZCompositeEntity (data: void)
			case -1895895177: // ZEntity (data: void)
			case -1910763817: // ZEntity (data: void)
			case -1915321744: // ZEntity (data: void)
			case -1928553780: // ZMenuSoundControllerEntity (data: void)
			case -1945579104: // ZEntity (data: "Triggered"), (data: "StageActive")
			case -1965092106: // ZEntity (data: void)
			case -1985984869: // ZUIMapControllerEntity (data: void)
			case -1986069737: // ZEntity (data: void)
			case -1999718269: // ZEntity (data: void)
			case -2013991646: // ZValueFloat_Set (data: float32)
			case -2017399598: // ZEntity (data: bool)
			case -2039424415: // ZEventTrack (data: void)
			case -2043592962: // ZEntity (data: float32)
			case -2054374999: // ZCompositeEntity (data: float32)
			case -2062003256: // ZEntity (data: ZEntityRef)
			case -2067424040: // ZEntity (data: void)
			case -2085852099: // ZEntity (data: void)
			case -2103525802: // ZEntity (data: void)
			case -2112315580: // 
			case -2119750791: // ZMathMultiplyDivide (data: float32)
			case -2128352618: // ZEntity (data: "L2")
			case -2136102573: // ZEntity (data: void)
				// cba to order them manually anymore
			case 1055672457:
			case 1050280336:
			case 786902495:
			case 1408858392:
			case -1182262940:
			case -1940939528:
			case -1040838345:
			case -1092819537:
			case -1293974422:
			case -1989077684:
			case -1659968349:
			case 367000212:
			case 1745615803:
			case 83764816:
			case 927954381:
			case 2102031519:
			case 197827954:
			case 746633541:
			case 1667166751:
			case -232823970:
			case -1615459667:
			case 130104470:
			case 607230171:
			case 465660913:
			case 1810963393:
			case 315378891:
			case 1288377036:
			case 1072462684:
			case 620644750:
			case -1844692125:
			case 1216954092:
			case 1764558887:
			case -336133481:
			case -1754336644:
			case -1713424973:
			case -1508536457:
			case 307963197:
			case 1227760737:
			case 1225968056:
			case 590174460:
			case -1030825879:
			case -1903875009:
			case -462606922: // ZContractsGamestateEmitterEntity (data: ZDynamicObject)
			case 811610981: // ZCompositeEntity (data: void)
			case -43794944: // ZContractsGamestateEmitterEntity (data: ZDynamicObject)
			case 1025206181:
			case -1672469205:
			case -2035442526: // ZEntity(data: void)
			case 1866770608:
			case 300449051:
			case 698225228: // ZEntity (data: "NPCdist_SusEvent"), (data: "ST abort"), (data: "PlotRew-Challenge Comp.")
			case 2107441970:
			case 53640605:
			case -193275709:
			case 1814441712:
			case -129357127:
			case 105741800:
				// case -291562784: - enter crouch?
				// case -2101353226: // ZEntity (data: void) - item pick up?
			// place item (briefcase
				// OnPlaced from ZEntity (data: void) - child of ZItem - free win (fired multiple)
				// 406361362 - ZCompositeEntity (data: void)
			// enter climb ladder
			case -908111829: // ZEntity(data: void)
			// climb down
			case 1356509126:
			// level change
			//case -1633404151: // ZUIMapConfig (data: "SLICE_02")
			// get off lander
			case 902815389:
			// unholster weapon
				// HM_WeaponEquipped from ZWeaponSoundSetupEntity (data: void)
				/*
				| Props:
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchWeaponClass
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchWeaponFamily
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchFireType
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchAudioHeadType
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchAudioTailType
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchExoticType
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchHighPassFilter
				|  TEntityRef<ZSignalSwitchEntity_void> m_rSwitchLowPassFilter
				| Interfaces:
				|  ZWeaponSoundSetupEntity
				|  IEntity
				|  void
				|  Parent Props:
				|    ZSpatialEntity.ERoomBehaviour m_eRoomBehaviour
				|    bool m_bForceVisible
				|    SMatrix43 m_mTransform
				|    bool m_bVisible
				|    bool m_bIsPrivate
				|    bool m_bVisibleInBoxReflection
				|    uint8 m_nViewportVisibility
				|    TEntityRef<ZSpatialEntity> m_eidParent
				|    TResourcePtr 4146518947
				|    TResourcePtr 4252183518
				|    TResourcePtr 2165728840
				|    TResourcePtr 1969181366
				|    TResourcePtr 3319749964
				|    TResourcePtr 2530662484
				|    TResourcePtr 3291758847
				|    TResourcePtr 2224538749
				|    ZEntityRef m_rHolder
				|    ZString 1787637954
				|    float32 1951200754
				|    float32 3980764232
				|    float32 2588054750
				|    float32 69599613
				|    float32 1931555307
				|    float32 964779477
				|    float32 2693270639
				|    float32 3616488697
				|    float32 1240181082
				|    float32 1055693260
				|    TResourcePtr 1185988173
				|    TResourcePtr 656729747
				|    TResourcePtr 2781052016
				|  Parent Interfaces:
				|    ZSpatialEntity
				|    IEntity
				|    ZWeaponSoundSetupEntity
				|    void
				|    Parent Props:
				|      ZEntityRef m_ScopeCrossHair
				|      bool m_bConnectsToTarget
				|      float32 m_fMass
				|      TResourcePtr 3424998006
				|      ZRuntimeResourceID m_ridClipTemplate
				|      TEntityRef<IAnimPlayerEntity> 232855195
				|      TEntityRef<ZHM5WeaponHandPosBox> m_LeftHandPos
				|      eWeaponType m_WeaponType
				|      ZEntityRef m_ScopePosition
				|      bool m_bScopedWeapon
				|      float32 m_fMuzzleEnergyMultiplier
				|      ZEntityRef m_CartridgeEject
				|      ZEntityRef m_MuzzleExit
				|      ZEntityRef m_AmmoProperties
				|      bool m_bRemovePhysics
				|      float32 m_fCartridgeEjectForceMultiplier
				|      TEntityRef<IVariationResourceEntity> m_pVariationResource
				|      EWeaponAnimationCategory m_eAnimationCategory
				|      TEntityRef<ZGeomEntity> m_rGeomentity
				|      TEntityRef<ZSpatialEntity> m_PosBackAttach
				|      ZString 1787637954
				|      TEntityRef<ZSpatialEntity> 2190610389
				|      ZEntityRef m_rHolder
				|      EDisposalType m_DisposalType
				|      ZEntityRef m_rSpecialImpactAct
				|      ZEntityRef m_rSuperSpecialTriggerEffect
				|      TEntityRef<ZSpatialEntity> m_Geom
				|      bool m_bKinematic
				|      TArray<TEntityRef<ZSubaction>> m_aSubactions
				|      TEntityRef<IChildNetworkEntity> m_pIdleAnimNetworkEntity
				|      bool m_bRandomStartState
				|      TEntityRef<ZSpatialEntity> m_rBeaconSpatial
				|      TEntityRef<ZSpatialEntity> m_PosPlacementAttach
				|      TEntityRef<ZBoxVolumeEntity> m_PlacementVolume
				|      EActorSoundDefs m_eInvestigateSoundDef
				|      TEntityRef<IBoolCondition> m_rVisibleInInventory
				|      TArray<TEntityRef<ZSpatialEntity>> m_aPromptPositions
				|      TEntityRef<ZEventConsumerCollection> m_EventConsumerCollection
				|      TResourcePtr 1185988173
				|      TResourcePtr 2224538749
				|      TResourcePtr 3291758847
				|      TResourcePtr 2530662484
				|      TResourcePtr 3319749964
				|      TResourcePtr 1969181366
				|      ERenderGlowTypes m_eGeomGlowType
				|      TResourcePtr 2781052016
				|      TResourcePtr 656729747
				|      TEntityRef<ZSpatialEntity> m_PosHandAttach
				|    Parent Interfaces:
				|      ZEntity
				|      IEntity
				|      IItemBase
				|      IFirearm
				|      IPhysicsAccessor
				|      IDynamicPhysics
				|      IItemWeapon
				|      IItem
				|      ZHM5ItemWeapon
				|      ZHM5Item
				|      void
				*/
			case 1019863584:
			// fire headshot kill guard
				// TriggerPressed from ZHM5ItemWeapon (data: void)
			case 1290557591: // sound related
				/*
				| PIN: PlayerAllShots from ZEntity (data: void) - fired twice
				| Props:
				|  ZEntityRef m_ScopeCrossHair
				|  bool m_bConnectsToTarget
				|  float32 m_fMass
				|  TResourcePtr 3424998006
				|  ZRuntimeResourceID m_ridClipTemplate
				|  TEntityRef<IAnimPlayerEntity> 232855195
				|  TEntityRef<ZHM5WeaponHandPosBox> m_LeftHandPos
				|  eWeaponType m_WeaponType
				|  ZEntityRef m_ScopePosition
				|  bool m_bScopedWeapon
				|  float32 m_fMuzzleEnergyMultiplier
				|  ZEntityRef m_CartridgeEject
				|  ZEntityRef m_MuzzleExit
				|  ZEntityRef m_AmmoProperties
				|  bool m_bRemovePhysics
				|  float32 m_fCartridgeEjectForceMultiplier
				|  TEntityRef<IVariationResourceEntity> m_pVariationResource
				|  EWeaponAnimationCategory m_eAnimationCategory
				|  TEntityRef<ZGeomEntity> m_rGeomentity
				|  TEntityRef<ZSpatialEntity> m_PosBackAttach
				|  ZString 1787637954
				|  TEntityRef<ZSpatialEntity> 2190610389
				|  ZEntityRef m_rHolder
				|  EDisposalType m_DisposalType
				|  ZEntityRef m_rSpecialImpactAct
				|  ZEntityRef m_rSuperSpecialTriggerEffect
				|  TEntityRef<ZSpatialEntity> m_Geom
				|  bool m_bKinematic
				|  TArray<TEntityRef<ZSubaction>> m_aSubactions
				|  TEntityRef<IChildNetworkEntity> m_pIdleAnimNetworkEntity
				|  bool m_bRandomStartState
				|  TEntityRef<ZSpatialEntity> m_rBeaconSpatial
				|  TEntityRef<ZSpatialEntity> m_PosPlacementAttach
				|  TEntityRef<ZBoxVolumeEntity> m_PlacementVolume
				|  EActorSoundDefs m_eInvestigateSoundDef
				|  TEntityRef<IBoolCondition> m_rVisibleInInventory
				|  TArray<TEntityRef<ZSpatialEntity>> m_aPromptPositions
				|  TEntityRef<ZEventConsumerCollection> m_EventConsumerCollection
				|  TResourcePtr 1185988173
				|  TResourcePtr 2224538749
				|  TResourcePtr 3291758847
				|  TResourcePtr 2530662484
				|  TResourcePtr 3319749964
				|  TResourcePtr 1969181366
				|  ERenderGlowTypes m_eGeomGlowType
				|  TResourcePtr 2781052016
				|  TResourcePtr 656729747
				|  TEntityRef<ZSpatialEntity> m_PosHandAttach
				| Interfaces:
				|  ZEntity
				|  IEntity
				|  IItemBase
				|  IFirearm
				|  IPhysicsAccessor
				|  IDynamicPhysics
				|  IItemWeapon
				|  IItem
				|  ZHM5ItemWeapon
				|  ZHM5Item
				|  void
				*/
				// ShotBegin - ZWeaponSoundSetupEntity (data: void)
				// Discharge_ShotSilenced - ZAISoundEventObjectProxyProbe_FirearmDischarge (data: void)
				// EjectCartridgeHitman
			case 1332335305: // ZLinkedEntity (data: void)
			case 2034673660: // ZSpatialEntity (data: float32) child of ZWeaponSoundSetupEntity, ZHM5Item, fired multiple times
			//--------
			case -1281814091:
			case -229749455:
			case -557889416:
			case 391580448:
			case -1777854548:
			case 266425415:
			case 1413943265:
			case -1056662080:
			case 890198583:
			case 926649696:
			case 294438097:
			case 367579676:
			case 818029569:
			case -196240032:
			case 1346445482:
			case 1559263426:
			case 1743628183:
			case -1810941947:
			case 391650318:
			case -1973613329:
			case 3480739:
			case 908098612:
			case -802474842:
			case 905823080: // (data: ZRepositoryID)
			case -1031334549:
			case 2074703694:
			case -2061100126:
			case 172688250:
			case -615215956:
			case 1060043035:
			case 1401231302:
			case 640249298:
			case -854908369:
			case 1405826326:
			case -1518524019:
			case -808252192:
			case -318099271:
			case -1428458509:
			case 1704424960:
			case 406361362:
			case 1901561271:
			case 719193945:
			case 398854508:
			case 2328200:
			case 801910836:
				// WeaponStartReload
			case 1158123228:
			case -215566393:
			case 680813622:
			case 1990439715:
			case -1315545446:
			case -367005483:
			case 1881503160:
			case -821723473:
			case -24459860:
			case 802783049:
			case 1983877692:
			case -681415085:
			case 13655135:
			case 1542653414:
			case -1848617060:
			case 1255368527:
			case 509323699:
			case 1893966515:
			case -2141485201:
			case -388421209:
			case -1355959696:
			case 2075782866:
			case 1316936680:
			case 1770243176:
			case 894003887:
			case -1197939925:
			case 1942493734:
			case 1789867337:
			case -777586257:
			case -1806081432:
			case -197409945:
			case -390935135:
			case 1444905342:
			case 2125379121:
			case 157760052: // IActor
			case -1724102975:
			case 1903192347:
			case -377375279:
			case -429407685:
			case 1855284431:
			case -727064886:
			case -805149983:
			case 89770573:
			case 1545443214:
			case 918672854:
			case -1664564677:
			case -1407712754:
			case -2039267478:
			case -936966498:
			case -107809259:
			case -424849847:
			case -608465234:
			case -637366345:
			case -155940248:
			case 1343317345:
			case -1032651361:
			case -145533265:
			case -1898542098:
			case 86424699:
			case 323428690:
			case 828291008:
			case 980248263:
			case -1089068114:
			case -597598179:
			case -1570624266:
			case -1132788442:
			case 1113137131:
			case -710517353:
			case -637732865:
			case 172789393:
			case 1165547372:
			case -1416447954:
			case 1323991424: // ZEntity (data: "TrgtAppear")
			case 1149412963:
			case 621812145:
			case 1230024838:
			case 214192701:
			case -2085613975:
			case -549217609:
			case 780777879:
			case -672301174:
			case 911321668:
			case 1836656427:
			case -1183905124:
			case -1675516562:
			case -1397239191:
			case -994240909:
			case -1481421327:
			case 359383687:
			case 1162880474:
			case -1651064648:
			case 53117872:
			case -2001530837:
			case 261544537:
			case -2085616854:
			case 1614324719:
			case 1481503785:
			// take back disguise/suit bundle
				// ButtonHoldComplete from ZHM5InputActionEventEmitter (data: void)
				return true;
		}
		switch (static_cast<ZHMPin>(pin)) {
			case ZHMPin::NavigateSlots: // inventory slot navigation - maybe?
			case ZHMPin::WeaponUnEquip: // fires a lot
			case ZHMPin::WeaponUnEquipped: // fires a lot
			case ZHMPin::WeaponUnEquipLegal: // fires a lot
			case ZHMPin::WeaponEquip: // fires a lot
			case ZHMPin::NPCDead: // can be triggered by pacification??
			case ZHMPin::OnHolster: // fires a lot
			case ZHMPin::OwnedByHitman: // fires a lot at start
			case ZHMPin::DeadBodySeenAccident: // fired a lot + we can get this easier
			case ZHMPin::HoldingIllegalWeapon: // fired a lot + there's an event
			case ZHMPin::DisguiseBlown: // fired a lot + there's an event
			case ZHMPin::DisguiseHealth: // fired a lot + idk what it is
			case ZHMPin::SpottedThroughDisguise: // ZGameStatsEvent could be interesting
			case ZHMPin::HitmanSpotted: // ZGameStatsEntityStealth could be interesting
			case ZHMPin::DisguiseBroken: // ZHUDAIGuide
			case ZHMPin::BlendInActivated: // no blending?
			case ZHMPin::BlendInStart: // no blending?
			case ZHMPin::InDisguise: // fired a lot
			case ZHMPin::WeaponEquipLegal: // ZWeaponSoundController, fired a bunch, maybe useful?
			case ZHMPin::WeaponEquipMelee: // ZWeaponSoundController, fired a bunch, maybe useful?
			case ZHMPin::WeaponPlayerEquipped: // ZHM5ItemCCWeapon, ZEntity, probably more useful
			case ZHMPin::Equipped: // ZHM5ItemCCWeapon, ZEntity, ???
			case ZHMPin::HMState_StartSneak: // could be usable, just annoyed by the log spam
			case ZHMPin::HMState_StopSneak: // could be usable, just annoyed by the log spam
			case ZHMPin::WeaponAimStop: // ZWeaponSoundController
			case ZHMPin::InvestigateCautious: // ZGameStatsEvent, maybe useful
			case ZHMPin::NPCAlerted: // ZCrowdEntity (data: SDynObstaclePinArg)
			case ZHMPin::NPCHasExclamationMark: // ZAIEventEmitterEntity, (data: bool), fired a lot... maybe
			case ZHMPin::WeaponEquipSinglePistol: // ZWeaponSoundController, (data: void)
			case ZHMPin::WeaponEquipIllegal: // ZWeaponSoundController
			case ZHMPin::HMState_OpenDoor: // bit repetitive
			case ZHMPin::WeaponEquipped: // bit repetitive
			case ZHMPin::EnterCover: // ZHM5HMStateSoundController, ZHM5InstinctSoundController, and other sound controllers
			case ZHMPin::ExitCover: // ^
			case ZHMPin::CoverUsed: // maybe but also repetitive
			case ZHMPin::OnHeroEnterCover: // maybe but also repetitive
			case ZHMPin::OnHeroLeaveCover: // ZCoverPlane
			case ZHMPin::HitmanInCoverBegin: // ZHM5CrowdReactionEntity
			case ZHMPin::HitmanInCoverEnd: // ZHM5CrowdReactionEntity
			case ZHMPin::RunUsed: // maybe but also repetitive
			case ZHMPin::WeaponFire: // ZWeaponSoundController
			case ZHMPin::PlayerEndBurstShot: // ZHM5ItemWeapon
			case ZHMPin::HM_HitNPC: // ZBulletImpactListenerEntity,
			case ZHMPin::HM_HitNPCAt: // ZBulletImpactListenerEntity,
			case ZHMPin::HMState_StartRun: // ZHM5HMStateSoundController, ZHM5HealthSoundController,
			case ZHMPin::HMState_CloseDoor:
			case ZHMPin::ChangedDisguise:
			case ZHMPin::TakingNewDisguise:
			case ZHMPin::EnterSniperMode:
			case ZHMPin::ExitSniperMode:
			case ZHMPin::IsNPC:
			case ZHMPin::NPCFirstBurstShot:
			case ZHMPin::BodyFound:
			case ZHMPin::BodyFoundPacify:
			case ZHMPin::BodyFoundPacifyId:
			case ZHMPin::ChangedDisguiseClean:
			case ZHMPin::IntegerCount:
			case ZHMPin::FloatCount:
			case ZHMPin::Attenuation:
			case ZHMPin::ActorEntered:
			case ZHMPin::ActorExited:
			case ZHMPin::Armed:
			case ZHMPin::Pitch:
			case ZHMPin::Show:
			case ZHMPin::Hide:
			case ZHMPin::DetectedPacified:
			case ZHMPin::StateExitedOrDisabled:
			case ZHMPin::ReactionTriggered:
			case ZHMPin::UnnoticedPacified:
			case ZHMPin::OnGreeting:
			case ZHMPin::OnTumble:
			case ZHMPin::OnSpawn:
			case ZHMPin::OnEnter: // ZSecuritySystemCamera (data: void)
			case ZHMPin::OnExitScopeMode:
			case ZHMPin::OnDrop:
			case ZHMPin::OnDropByHero:
			case ZHMPin::OnPressed:
			case ZHMPin::OnActionY:
			case ZHMPin::OnActionB:
			case ZHMPin::OnActionX:
			case ZHMPin::OnActionA:
			case ZHMPin::OnRecorded: // ZEntity (data: void)
			case ZHMPin::ExecutedData:
			case ZHMPin::PlaySound:
			case ZHMPin::Intensity:
			case ZHMPin::StealthKill:
			case ZHMPin::OnMurdered:
			case ZHMPin::OnGetEntityRef:
			case ZHMPin::OnDead:
			case ZHMPin::OnDying:
			case ZHMPin::OnTargetDead:
			case ZHMPin::StartExecuteKill:
			case ZHMPin::HitmanCCBegin:
			case ZHMPin::HitmanCCEnd:
			case ZHMPin::PageOpened:
			case ZHMPin::InstinctUnavailable:
			case ZHMPin::CC_Start:
			case ZHMPin::CC_Start_Hitman:
			case ZHMPin::CC_End:
			case ZHMPin::CC_End_Win:
			case ZHMPin::CC_Final_Impact:
			case ZHMPin::UpdatedPriority:
			case ZHMPin::EliminateUsed:
			case ZHMPin::HM_HitNPCCloseCombatShot:
			case ZHMPin::Weapon:
			case ZHMPin::Kill:
			case ZHMPin::TargetDied:
			case ZHMPin::WeaponPlayerUnEquipped:
			case ZHMPin::DeadlyThrowImpact:
			case ZHMPin::DataStart:
			case ZHMPin::IActor:
			case ZHMPin::Unequipped:
			case ZHMPin::Triggered:
			case ZHMPin::DoorClose:
			case ZHMPin::DoorCloseByHitmanFirst:
			case ZHMPin::DoorOpenByHitmanFirst:
			case ZHMPin::DisguiseBlendInActivated:
			case ZHMPin::ObjectiveCompleted:
			case ZHMPin::PrimaryObjectiveCompleted:
			case ZHMPin::PrimaryObjectiveFailed:
			case ZHMPin::ScoreCommon:
			case ZHMPin::Trespassing:
			case ZHMPin::WeaponUnEquipIllegal:
			case ZHMPin::CombatHitmanSpotted:
			case ZHMPin::Failed:
			case ZHMPin::OnThrown:
			case ZHMPin::OnFailed:
			case ZHMPin::OnCombatStarted:
			case ZHMPin::OnChallengeCompleted:
			case ZHMPin::OnCombatEnded:
			case ZHMPin::OnNoReceiversRegistered:
			case ZHMPin::OnIActor:
			case ZHMPin::OnDestroyed:
			case ZHMPin::OnReadySetpiece:
			case ZHMPin::OnPickup:
			case ZHMPin::OnWarning:
			case ZHMPin::OnGetItem:
			case ZHMPin::OnItemConsumed:
			case ZHMPin::OnTargetOutOfRange:
			case ZHMPin::NoPageTabAvailable:
			case ZHMPin::SomeonePacified:
			case ZHMPin::KillData:
			case ZHMPin::RepositoryID:
			case ZHMPin::RoomID:
			case ZHMPin::RoomId:
			case ZHMPin::ActorId:
			case ZHMPin::ActorName:
			case ZHMPin::ActorType:
			case ZHMPin::DeathContext:
			case ZHMPin::HitmanGuardKill:
			case ZHMPin::DeadlyThrowOn:
			case ZHMPin::DeadlyThrowOff:
			case ZHMPin::DeadlyThrowActivated:
			case ZHMPin::ReleasedItem:
			case ZHMPin::HitmanPush:
			case ZHMPin::HitmanPushSignal:
			case ZHMPin::HitmanSuspiciousSignal:
			case ZHMPin::HitmanNotSuspiciousSignal:
			case ZHMPin::HitmanGuardSilenced:
			case ZHMPin::HM_HitNPCHeadShot:
			case ZHMPin::HM_HitNPCHeadShot_IActor:
			case ZHMPin::HM_HitNPCHeadShotAt:
			case ZHMPin::KillerHero:
			case ZHMPin::KillItemInstanceId:
			case ZHMPin::KillItemRepositoryId:
			case ZHMPin::KillItemCategory:
			case ZHMPin::WeaponAimStart:
			case ZHMPin::DataEnd:
			case ZHMPin::Dead:
			case ZHMPin::Fired: // not what it sounds like
			case ZHMPin::OnFireProjectiles:
			case ZHMPin::OnFireProjectilesLocal:
			case ZHMPin::EnterAimAt:
			case ZHMPin::ExitAimAt:
			case ZHMPin::HitmanAimBegin:
			case ZHMPin::HitmanAimEnd:
			case ZHMPin::PageBack:
			case ZHMPin::OnStateChanged:
			case ZHMPin::PageClosed:
			case ZHMPin::UpToWhite:
			case ZHMPin::Impact:
			case ZHMPin::HitmanBumping:
			case ZHMPin::OnJoinNPC:
			case ZHMPin::ShotsPerSecondNPC:
			case ZHMPin::ShotsPerSecondHero:
			case ZHMPin::HaveActiveParticles:
			case ZHMPin::DeadBodySeenMurder:
			case ZHMPin::DeadBodySeenMurderId:
			case ZHMPin::BodyFoundMurder:
			case ZHMPin::HitmanCivilianKill:
			case ZHMPin::NPC_HitHM:
			case ZHMPin::NPC_HitHMAt:
			case ZHMPin::UnsetCurrentAmbience:
			case ZHMPin::OnBusy:
			case ZHMPin::OnLast:
			case ZHMPin::OnCastRole:
			case ZHMPin::VisiblyArmed:
			case ZHMPin::StateEntered:
			case ZHMPin::IsInState:
			case ZHMPin::OnEntered: // ZHM5DisguiseSafeZoneEntity, ZCompositeEntity, ZSpatialEntity, ...
			case ZHMPin::InLoop:
			case ZHMPin::OnNext:
			case ZHMPin::EnterSafeZoneAny:
			case ZHMPin::ExitSafeZoneAny:
			case ZHMPin::LastEnemyKilled:
			case ZHMPin::EnemiesIsInCombat:
			case ZHMPin::HitmanTrespassingSpotted:
			case ZHMPin::OnReleased:
			case ZHMPin::Generic00:
			case ZHMPin::Generic01:
			case ZHMPin::Generic02:
			case ZHMPin::Generic03:
			case ZHMPin::Generic04:
			case ZHMPin::Generic05:
			case ZHMPin::Generic06:
			case ZHMPin::Generic07:
			case ZHMPin::ObjectEvent01:
			case ZHMPin::ObjectEvent02:
			case ZHMPin::ObjectEvent03:
			case ZHMPin::ObjectEvent04:
			case ZHMPin::OnLeft:
			case ZHMPin::BlendInStop:
			case ZHMPin::OnLeaving:
			case ZHMPin::NPCCivilianAlerted:
			case ZHMPin::NPCCivilianScared:
			case ZHMPin::OnAbort:
			case ZHMPin::Death:
			case ZHMPin::IsTarget:
			case ZHMPin::IsHeadshot:
			case ZHMPin::KillType:
			case ZHMPin::ActorPosition:
			case ZHMPin::Actor:
			case ZHMPin::PacifiedData:
			case ZHMPin::ThrowArcOn:
			case ZHMPin::ThrowArcOff:
			case ZHMPin::LocaleChanged:
			case ZHMPin::AngleLookAt:
			case ZHMPin::SyncBeat:
			case ZHMPin::SyncBar:
			case ZHMPin::TextLocaleChanged:
			case ZHMPin::Speaking:
			case ZHMPin::HMFootstepMaterialChanged:
			case ZHMPin::Keyword:
			case ZHMPin::PageOpen:
			case ZHMPin::IdleStart:
			case ZHMPin::OnSet:
			case ZHMPin::OnReset:
			case ZHMPin::Selected:
			case ZHMPin::OnEntering:
			case ZHMPin::OnActorChanged:
			case ZHMPin::OnStarted:
			case ZHMPin::OnActivated:
			case ZHMPin::OnDeactivate:
			case ZHMPin::Deactivated:
			case ZHMPin::InactiveStage:
			case ZHMPin::TriggerBeforeRaycast:
			case ZHMPin::OnPositionReached:
			case ZHMPin::HMMovementIndex:
			case ZHMPin::HMState_LeftStep:
			case ZHMPin::HMState_RightStep:
			case ZHMPin::OnEvent:
			case ZHMPin::OnEnabled:
			case ZHMPin::OnReached:
			case ZHMPin::OnInterrupted:
			case ZHMPin::SoundTensionAmbient:
			case ZHMPin::NPCHasQuestionMark:
			case ZHMPin::Enabled:
			case ZHMPin::OutX:
			case ZHMPin::OutY:
			case ZHMPin::OutZ:
			case ZHMPin::OnGet:
			case ZHMPin::CanNotOpenCPDoor:
			case ZHMPin::CurrentGait:
			case ZHMPin::CurrentHealth:
			case ZHMPin::OutsideMonitorDistance:
			//case ZHMPin::DisguiseHealth: // ?
			case ZHMPin::OnFracture:
			case ZHMPin::OnAnchorsLost:
			case ZHMPin::OnInitialFracture:
			case ZHMPin::OnInitialDetach:
			case ZHMPin::OnAttach:
			case ZHMPin::OnAttached:
			case ZHMPin::OnAttachToNPC:
			case ZHMPin::OnAttachToHitman:
			case ZHMPin::OnDetach:
			case ZHMPin::OnDetached:
			case ZHMPin::EnemiesIsAlerted:
			case ZHMPin::EnemiesIsAlertedArmed:
			case ZHMPin::EnemiesIsEngaged:
			case ZHMPin::SyncUserCue:
			case ZHMPin::SendSourcePosition:
			case ZHMPin::AudibleAttentionMax:
			case ZHMPin::AudibleAttentionMaxPan:
			case ZHMPin::AttentionMax:
			case ZHMPin::AttentionMaxPan:
			case ZHMPin::TrespassingAttentionMax:
			case ZHMPin::TrespassingAttentionMaxPan:
			case ZHMPin::DisguiseAttentionMax:
			case ZHMPin::DisguiseAttentionMaxPan:
			case ZHMPin::RepositoryId:
			case ZHMPin::HidingObjectivesBar:
			case ZHMPin::CutSequenceEnded:
			case ZHMPin::ChannelA:
			case ZHMPin::ChannelB:
			case ZHMPin::Activated:
			case ZHMPin::Insideness:
			case ZHMPin::ActiveStage:
			case ZHMPin::OnDisabled:
			case ZHMPin::Disabled:
			case ZHMPin::IsLastTriggeredAndNotTracked:
			case ZHMPin::OnAlive:
			case ZHMPin::OnMaxSightAttentionToPlayer:
			case ZHMPin::SetImageRID:
			case ZHMPin::GlowColor1:
			case ZHMPin::GlowColor2:
			case ZHMPin::ReportOwner:
			case ZHMPin::OnTargetInRange:
			case ZHMPin::OnValueChanged:
			case ZHMPin::GlowType:
			case ZHMPin::SetOpenCalled:
			case ZHMPin::CoverDisabled:
			case ZHMPin::OnAvailable:
			case ZHMPin::ClothBundleSpawned:
			case ZHMPin::InTrespassArea:
			case ZHMPin::InTrespassEntryArea:
			case ZHMPin::ParentRepositoryId:
			case ZHMPin::NumOccupiedSpots:
			case ZHMPin::NumOccupiedSpotsMale:
			case ZHMPin::NumOccupiedSpotsFemale:
			case ZHMPin::NumOccupiedSpotsMalePercent:
			case ZHMPin::ItemReady:
			case ZHMPin::ItemSpawned:
			case ZHMPin::OnSetItem:
			case ZHMPin::Keyword2:
			case ZHMPin::Keyword3:
			case ZHMPin::PromptPositionIndex:
			case ZHMPin::RayLength:
			case ZHMPin::StepCounter:
			case ZHMPin::LoopCounter:
			case ZHMPin::OnFirst:
			case ZHMPin::OnStep:
			case ZHMPin::DisplayingObjectivesBarWithoutChanges:
			case ZHMPin::OnActDone:
			case ZHMPin::OnItemSet:
			case ZHMPin::OnCompleted:
			case ZHMPin::OpenCalled:
			case ZHMPin::SendValue:
			case ZHMPin::CrossForward:
			case ZHMPin::CrossBackward:
			case ZHMPin::OnActorReleased:
			case ZHMPin::SendDestinationPosition:
			case ZHMPin::SendEventName:
			case ZHMPin::OnAborted:
			case ZHMPin::Aborted:
			case ZHMPin::OnFree:
			case ZHMPin::OnEffectActivated:
			case ZHMPin::OnEffectDeactivated:
			case ZHMPin::FloatValue:
			case ZHMPin::OnShow:
			case ZHMPin::Changed:
			case ZHMPin::FocusGained:
			case ZHMPin::OnDramaNewBehavior:
			case ZHMPin::OnEnterRole:
			case ZHMPin::OnPauseRole:
			case ZHMPin::OnResumingRole:
			case ZHMPin::OnDramaResuming:
			case ZHMPin::OnReadyActor:
			case ZHMPin::OnReadyActorAsEntity:
			case ZHMPin::OnWrap:
			case ZHMPin::GetDelayValue:
			case ZHMPin::OnExitRole:
			case ZHMPin::ControllerHintOpened:
			case ZHMPin::ControllerHintClosed:
			case ZHMPin::CrowdActStarted:
			case ZHMPin::CrowdActEnded:
			case ZHMPin::CrowdActorSelected:
			case ZHMPin::CrowdActorSelectedID:
			case ZHMPin::CrowdActorDeselected:
			case ZHMPin::CrowdActorDeselectedID:
			case ZHMPin::CrowdActorSelectionFailed:
			case ZHMPin::CrowdDensityTotal:
			case ZHMPin::CrowdAlertNearestActor:
			case ZHMPin::CrowdAlertNearestActor_Back:
			case ZHMPin::CrowdAlertNearestActor_Left:
			case ZHMPin::CrowdAlertNearestActor_Right:
			case ZHMPin::CrowdAmbientNearestActor:
			case ZHMPin::CrowdAmbientNearestActor_Back:
			case ZHMPin::CrowdAmbientNearestActor_Left:
			case ZHMPin::CrowdAmbientNearestActor_Right:
			case ZHMPin::CrowdAmbientRatio:
			case ZHMPin::CrowdAmbientRatio_Back:
			case ZHMPin::CrowdAmbientRatio_Left:
			case ZHMPin::CrowdAmbientRatio_Right:
			case ZHMPin::CrowdDownNearestActor:
			case ZHMPin::CrowdDownNearestActor_Back:
			case ZHMPin::CrowdDownNearestActor_Left:
			case ZHMPin::CrowdDownNearestActor_Right:
			case ZHMPin::CrowdScaredNearestActor:
			case ZHMPin::CrowdScaredNearestActor_Back:
			case ZHMPin::CrowdScaredNearestActor_Left:
			case ZHMPin::CrowdScaredNearestActor_Right:
			case ZHMPin::OnPauseDrama:
			case ZHMPin::OnPause:
			case ZHMPin::OnActorSet:
			case ZHMPin::OnCurrent:
			case ZHMPin::OnNotCurrent:
			case ZHMPin::ActEvent4001:
			case ZHMPin::ActEvent4002:
			case ZHMPin::ActEvent4003:
			case ZHMPin::ActEvent4004:
			case ZHMPin::ActEvent4005:
			case ZHMPin::ActEvent4006:
			case ZHMPin::ActEvent4007:
			case ZHMPin::OnNoItem:
			case ZHMPin::OnTrue:
			case ZHMPin::OnFalse:
			case ZHMPin::Output:
			case ZHMPin::Output1:
			case ZHMPin::Output2:
			case ZHMPin::Output3:
			case ZHMPin::Output4:
			case ZHMPin::Output5:
			case ZHMPin::Color:
			case ZHMPin::Duration:
			case ZHMPin::StopEventEnded:
			case ZHMPin::AimLookAt:
			case ZHMPin::Lerp:
			case ZHMPin::Result:
			case ZHMPin::Power:
			case ZHMPin::DiffusePower:
			case ZHMPin::OnValue:
			case ZHMPin::TimeOut:
			case ZHMPin::PollValue:
			case ZHMPin::GetValue:
			case ZHMPin::Done:
			case ZHMPin::FilterOut:
			case ZHMPin::RemovedKeyword:
			case ZHMPin::ReactionTriggeredAtPos:
			case ZHMPin::On:
			case ZHMPin::Off:
			case ZHMPin::Count:
			case ZHMPin::Out1:
			case ZHMPin::Out2:
			case ZHMPin::Out3:
			case ZHMPin::Out4:
			case ZHMPin::Out5:
			case ZHMPin::Out6:
			case ZHMPin::Out7:
			case ZHMPin::Out8:
			case ZHMPin::Out00:
			case ZHMPin::Out01:
			case ZHMPin::Out02:
			case ZHMPin::Out03:
			case ZHMPin::Out04:
			case ZHMPin::Out05:
			case ZHMPin::Out06:
			case ZHMPin::Out07:
			case ZHMPin::Out08:
			case ZHMPin::IdleStop:
			case ZHMPin::InstinctTimeMultiplier:
			case ZHMPin::ForwardVector:
			case ZHMPin::BackwardVector:
			case ZHMPin::PrincipalTargetDistance:
			case ZHMPin::EventOccurred:
			case ZHMPin::EventReceived:
			case ZHMPin::Distance:
			case ZHMPin::Trigger:
			case ZHMPin::BulletFlyByHitman:
			case ZHMPin::Closed:
			case ZHMPin::MenuClosed:
			case ZHMPin::OnActorCast:
			case ZHMPin::SomeoneScared: //ZCrowdEntity, (data: SDynObstaclePinArg)
			case ZHMPin::SomeoneDied: // ZCrowdEntity, (data: SDynObstaclePinArg)
			case ZHMPin::ShotFiredIntoCrowd:
			case ZHMPin::IsPlayer:
			case ZHMPin::Play:
			case ZHMPin::Stop:
			case ZHMPin::CrowdCulledRatio:
			case ZHMPin::CrowdCulledRatio_Back:
			case ZHMPin::CrowdCulledRatio_Left:
			case ZHMPin::CrowdCulledRatio_Right:
			case ZHMPin::CrowdDensity1:
			case ZHMPin::CrowdDensity1_Back:
			case ZHMPin::CrowdDensity1_Left:
			case ZHMPin::CrowdDensity1_Right:
			case ZHMPin::CrowdAlertRatio:
			case ZHMPin::CrowdAlertRatio_Back:
			case ZHMPin::CrowdAlertRatio_Left:
			case ZHMPin::CrowdAlertRatio_Right:
			case ZHMPin::CrowdScaredRatio:
			case ZHMPin::CrowdScaredRatio_Back:
			case ZHMPin::CrowdScaredRatio_Left:
			case ZHMPin::CrowdScaredRatio_Right:
			case ZHMPin::OnGetAccessoryItem:
			case ZHMPin::OnTriggeredEvent01:
			case ZHMPin::OnTriggeredEvent02:
			case ZHMPin::OnTriggeredEvent03:
			case ZHMPin::OnTriggeredEvent04:
			case ZHMPin::OnTriggeredEvent05:
			case ZHMPin::OnTriggeredEvent06:
			case ZHMPin::OnTriggeredEvent07:
			case ZHMPin::OnTriggeredEvent08:
			case ZHMPin::OnItemFocus:
			case ZHMPin::OnItemChanged:
			case ZHMPin::OnActTimeout:
			case ZHMPin::OnEnterDrama:
			case ZHMPin::OnStart:
			case ZHMPin::OnIActorChanged:
			case ZHMPin::OnGetSetpieceUsed:
			case ZHMPin::OnGetItemUsed:
			case ZHMPin::OnResumed:
			case ZHMPin::OnItemGrabbed:
			case ZHMPin::Completed:
			case ZHMPin::MinDimension:
			case ZHMPin::MaxDimension:
			case ZHMPin::Glow:
			case ZHMPin::GameTension:
			case ZHMPin::GameTensionAmbient:
			case ZHMPin::GameTensionAgitated:
			case ZHMPin::GameTensionAlertedHigh:
			case ZHMPin::GameTensionAlertedLow:
			case ZHMPin::GameTensionArrest:
			case ZHMPin::GameTensionCombat:
			case ZHMPin::GameTensionHunting:
			case ZHMPin::GameTensionGuardHM:
			case ZHMPin::GameTensionGuardHM_Agitated:
			case ZHMPin::GameTensionGuardHM_AlertedHigh:
			case ZHMPin::GameTensionGuardHM_AlertedLow:
			case ZHMPin::GameTensionGuardHM_Ambient:
			case ZHMPin::GameTensionGuardHM_Arrest:
			case ZHMPin::GameTensionGuardHM_Combat:
			case ZHMPin::GameTensionGuardHM_Hunting:
			case ZHMPin::GameTensionCivilian:
			case ZHMPin::GameTensionCivilian_Combat:
			case ZHMPin::GameTensionCivilian_Hunting:
			case ZHMPin::GameTensionCivilian_Ambient:
			case ZHMPin::GameTensionCivilian_Agitated:
			case ZHMPin::GameTensionCivilian_AlertedHigh:
			case ZHMPin::GameTensionCivilian_AlertedLow:
			case ZHMPin::GameTensionCivilian_Arrest:
			case ZHMPin::GameTensionCivilianHM:
			case ZHMPin::GameTensionCivilianHM_Ambient:
			case ZHMPin::GameTensionCivilianHM_Agitated:
			case ZHMPin::GameTensionCivilianHM_AlertedHigh:
			case ZHMPin::GameTensionCivilianHM_AlertedLow:
			case ZHMPin::GameTensionCivilianHM_Arrest:
			case ZHMPin::GameTensionCivilianHM_Combat:
			case ZHMPin::GameTensionCivilianHM_Hunting:
			case ZHMPin::AttentionOSDVisible:
			case ZHMPin::MusicStarted:
			case ZHMPin::HitmanVisibleWeaponBegin:
			case ZHMPin::CivilianGameTensionAlertedLow:
			case ZHMPin::EventRegistered:
			case ZHMPin::OnUnlocked:
			case ZHMPin::Executed:
			case ZHMPin::OnAbortEntering:
			case ZHMPin::WentIntoRange:
			case ZHMPin::WithinProximityChanged:
			case ZHMPin::CutSequenceStarted:
			case ZHMPin::OnPlay:
			case ZHMPin::X:
			case ZHMPin::Y:
			case ZHMPin::Z:
			case ZHMPin::LoadingTransitionDelayStarted:
			case ZHMPin::LoadingTransitionDelayEnded:
			case ZHMPin::PositionOutput:
			case ZHMPin::Same:
			case ZHMPin::Invert:
			case ZHMPin::GetTrue:
			case ZHMPin::GetFalse:
			case ZHMPin::Value:
			case ZHMPin::Negate:
			case ZHMPin::Else:
			case ZHMPin::OnReady:
			case ZHMPin::OnChange:
			case ZHMPin::AddedKeyword:
			case ZHMPin::LowClamped:
			case ZHMPin::HighClamped:
			case ZHMPin::Unclamped:
			case ZHMPin::Clamped:
			case ZHMPin::Abs:
			case ZHMPin::WentBelowMin:
			case ZHMPin::WentAboveMax:
			case ZHMPin::Deselected:
			case ZHMPin::ButtonPressed:
			case ZHMPin::SelectionChanged:
			case ZHMPin::PageSelectionChanged:
			case ZHMPin::NextPageTabSelected:
			case ZHMPin::PreviousPageTabSelected:
			case ZHMPin::MainEventEnded:
			case ZHMPin::DistanceChanged:
			case ZHMPin::Opened:
			case ZHMPin::TotalCoverValue:
			case ZHMPin::TurnLightOn:
			case ZHMPin::MinValue:
			case ZHMPin::MaxValue:
			case ZHMPin::MinIndex:
			case ZHMPin::MaxIndex:
			case ZHMPin::PrincipalTargetAngleHoriz:
			case ZHMPin::PrincipalTargetAngleVert:
			case ZHMPin::PrincipalTargetIndex:
			case ZHMPin::PrincipalTargetVisible:
			case ZHMPin::MaxNumberOfTargets:
			case ZHMPin::SecurityCameraAttentionMax:
			case ZHMPin::SecurityCameraAttentionMaxPan:
			case ZHMPin::Started:
			case ZHMPin::Stopped:
			case ZHMPin::Inside:
			case ZHMPin::Outside:
			case ZHMPin::OnWake:
			case ZHMPin::OnSleep:
			case ZHMPin::OnImpact:
			case ZHMPin::OnImpactInfo:
			case ZHMPin::OutSignal:
			case ZHMPin::InDisguiseZone:
			case ZHMPin::NPCHasWhiteDot:
			case ZHMPin::RotationOutput:
			case ZHMPin::SetGlowType:
			case ZHMPin::Then:
			case ZHMPin::Run:
			case ZHMPin::OutputEvent:
			case ZHMPin::OnStopped:
			case ZHMPin::DoorClosed:
			case ZHMPin::LadderSlideStop:
			case ZHMPin::LadderSlideStart:
			case ZHMPin::CoverEnabled:
			case ZHMPin::OnLeaveDrama:
			case ZHMPin::OnDone:
			case ZHMPin::OnEnd:
			case ZHMPin::OnUnpause:
			case ZHMPin::OnUnpauseDrama:
			case ZHMPin::OnTerminate:
			case ZHMPin::Weight:
			case ZHMPin::CurrentAmbience:
			case ZHMPin::DoorOpenByAny:
			case ZHMPin::DoorOpenByAnyIn:
			case ZHMPin::DoorOpenByAnyOut:
			case ZHMPin::DoorCloseByAny:
			case ZHMPin::DoorCloseNoOperator:
			case ZHMPin::CloseCalled:
			case ZHMPin::OnGetIActor:
			case ZHMPin::OnProjectionTurnedOn:
			case ZHMPin::OnProjectionTurnedOff:
			case ZHMPin::ClosestDistance:
			case ZHMPin::WireDetach:
			case ZHMPin::OnActivate:
			case ZHMPin::OnLostOwnership:
			case ZHMPin::OnInvisible:
			case ZHMPin::FocusLost:
			case ZHMPin::OnBehaviorStarted:
			case ZHMPin::OnBehaviorEnded:
			case ZHMPin::OnCooldown:
			case ZHMPin::OnStop:
			case ZHMPin::AudioDone:
			case ZHMPin::SubtitleChanged:
			case ZHMPin::GentlePush:
			case ZHMPin::GentlePushSignal:
			case ZHMPin::HardPush:
			case ZHMPin::HardPushSignal:
			case ZHMPin::HMState_StopRun:
			//case ZHMPin::HMState_StopSneak:
			//case ZHMPin::HMState_StartSneak:
			case ZHMPin::Vector2:
			case ZHMPin::Vector3:
			case ZHMPin::AddedToPhysicsWorld:
			case ZHMPin::OwnedByNPC:
			case ZHMPin::EnablePickup:
			case ZHMPin::OnRelease:
			case ZHMPin::OnVisible:
			case ZHMPin::OutRGBA:
			case ZHMPin::OnSetVisible:
			case ZHMPin::OnSetKinematic:
			case ZHMPin::OnBecomeInvisible:
			case ZHMPin::OnBecomeVisible:
			case ZHMPin::OnSelected:
			case ZHMPin::OnSelectedSkin:
			case ZHMPin::OnSelectedScope:
			case ZHMPin::OnSelectedMuzzleExtension:
			case ZHMPin::Item:
			case ZHMPin::ShotInterval:
			case ZHMPin::ShotsPerMinute:
			case ZHMPin::HitmanVisibleWeaponEnd:
			case ZHMPin::MenuOpened:
			case ZHMPin::ScoreFail:
				return true;
		}
		return false;
	}
};
