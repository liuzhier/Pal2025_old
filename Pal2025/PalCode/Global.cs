using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

using BOOL        = System.Boolean;
using CHAR        = System.Char;
using BYTE        = System.Byte;
using SHORT       = System.Int16;
using WORD        = System.UInt16;
using USHORT      = System.UInt16;
using INT         = System.Int32;
using DWORD       = System.UInt32;
using LONG        = System.Int64;
using QWORD       = System.UInt64;
using LPSTR       = System.String;
using FILE        = System.IO.File;

using static Pal.PalCommon;
using static Pal.PalPalette;

namespace Pal
{
   public unsafe class PalGlobal
   {
      public struct Files
      {
         public BYTE*      ABC_MKF;
         public BYTE*      BALL_MKF;
         public BYTE*      UI_MKF;
         public BYTE*      F_MKF;
         public BYTE*      FBP_MKF;
         public BYTE*      FIRE_MKF;
         public BYTE*      GOP_MKF;
         public BYTE*      MAP_MKF;
         public BYTE*      MGO_MKF;
         public BYTE*      PAT_MKF;
         public BYTE*      RGM_MKF;
         public BYTE*      RNG_MKF;
         public BYTE*      SOUNDS_MKF;
         public BYTE*      SSS_MKF;
      }

      //
      // 队伍信息
      //
      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct Party
      {
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct PartyInfo
         {
            public WORD       wPlayerRoleID;    // 队员编号
            public SHORT      x, y;             // 坐标
            public WORD       wFrame;           // 当前步伐帧（0～2）
            public WORD       wImageOffset;     // 未知，待补
         }

         fixed WORD wPartyInfo[5 * MAX_PLAYABLE_PLAYER_ROLES];

         public PartyInfo Get(WORD wPlayerID)
         {
            //
            // 固定指针，因为内存地址是动态变化的
            //
            fixed (WORD* lp = wPartyInfo)
            {
               //
               // 转化为 PARTY_PLAYER 指针
               //
               PartyInfo* lpParty = (PartyInfo*)lp;

               //
               // 将指针定位到指定队员的数据处
               //
               lpParty += wPlayerID;

               return *lpParty;
            }
         }
      }

      //
      // 玩家行走轨迹，用于其他团队成员跟随主要团队成员
      //
      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct Trail
      {
         public struct PlayerStep
         {
            public WORD    x, y;          // 坐标
            public WORD    wDirection;    // 面朝方向（第几组帧序列）
         }

         fixed WORD wPlayerStep[3 * MAX_PLAYABLE_PLAYER_ROLES];

         public PlayerStep Get(WORD wPlayerID)
         {
            //
            // 固定指针，因为内存地址是动态变化的
            //
            fixed (WORD* lp = wPlayerStep)
            {
               //
               // 转化为 TRAIL_PLAYER 指针
               //
               PlayerStep* lpTrail = (PlayerStep*)lp;

               //
               // 将指针定位到指定队员的数据处
               //
               lpTrail += wPlayerID;

               return *lpTrail;
            }
         }
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct Exp
      {
         public struct PlayerExp
         {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct ExpInfo
            {
               public WORD    wExp;          // 当前经验值
               public WORD    wReserved;
               public WORD    wLevel;        // 当前等级
               public WORD    wCount;
            }

            fixed WORD wExpInfo[4 * MAX_PLAYER_ROLES];

            public ExpInfo Get(WORD wPlayerID)
            {
               //
               // 固定指针，因为内存地址是动态变化的
               //
               fixed (WORD* lp = wExpInfo)
               {
                  //
                  // 转化为 EXPERIENCE 指针
                  //
                  ExpInfo* lpExp = (ExpInfo*)lp;

                  //
                  // 将指针定位到指定队员的数据处
                  //
                  lpExp += wPlayerID;

                  return *lpExp;
               }
            }
         }

         public PlayerExp     Primary;       // 队员主等级
         public PlayerExp     HP;            // HP 经验
         public PlayerExp     MP;            // MP 经验
         public PlayerExp     Attack;        // 武术
         public PlayerExp     MagicPower;    // 灵力
         public PlayerExp     Defense;       // 防御
         public PlayerExp     Dexterity;     // 身法（速度）
         public PlayerExp     Flee;          // 吉运（逃逸率）
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct PlayerRole
      {
         //
         // 基础数值
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct BaseInfo
         {
            fixed WORD wBase[MAX_PLAYER_ROLES];

            public WORD Get(WORD wPlayerID) => wBase[wPlayerID];
         }

         //
         // 装备
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct EquipInfo
         {
            public BaseInfo      Head;          // 头戴
            public BaseInfo      Shoulder;      // 披挂
            public BaseInfo      Body;          // 身穿
            public BaseInfo      Hand;          // 手持
            public BaseInfo      Foot;          // 脚穿
            public BaseInfo      Waist;         // 佩戴
         }

         //
         // 五灵抗性
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct ElementalResistanceInfo
         {
            public BaseInfo      Wind;       // 风系
            public BaseInfo      Thunder;    // 雷系
            public BaseInfo      Water;      // 水系
            public BaseInfo      Fire;       // 火系
            public BaseInfo      Earth;      // 土系
         }

         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct MagicInfo
         {
            fixed WORD wMagicInfo[MAX_PLAYER_MAGICS * MAX_PLAYER_ROLES];

            public BaseInfo Get(WORD wMagicID)
            {
               fixed (WORD* lp = wMagicInfo)
               {
                  BaseInfo* lpPlayer = (BaseInfo*)lp;

                  lpPlayer += wMagicID;

                  return *lpPlayer;
               }
            }

            public WORD Get(WORD wMagicID, WORD wPlayerID) => Get(wMagicID).Get(wPlayerID);
         }

         //
         // 队员音效
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct SoundInfo
         {
            public BaseInfo   DeathSound;       // 阵亡呐喊
            public BaseInfo   AttackSound;      // 平 A 呐喊
            public BaseInfo   WeaponSound;      // 武器击中敌人
            public BaseInfo   CriticalSound;    // 会心一击呐喊
            public BaseInfo   MagicSound;       // 施法呐喊
            public BaseInfo   CoverSound;       // 援护音效
            public BaseInfo   DyingSound;       // 虚弱倒地呐喊
         }

         public BaseInfo                     Avatar;                 // 肖像
         public BaseInfo                     BattleSpriteID;         // 战斗形象编号（隶属 F.MKF）
         public BaseInfo                     SpriteID;               // 场景形象编号（隶属 MGO.MKF）
         public BaseInfo                     Name;                   // 名称（隶属 WORD.DAT）
         public BaseInfo                     AttackAll;              // 是否能够攻击全体
         public BaseInfo                     Unknown1;               // 未知，待补
         public BaseInfo                     Level;                  // 修行
         public BaseInfo                     MaxHP;                  // HP 上限
         public BaseInfo                     MaxMP;                  // MP 上限
         public BaseInfo                     HP;                     // 当前 HP
         public BaseInfo                     MP;                     // 当前 MP
         public EquipInfo                    Equipment;              // 装备
         public BaseInfo                     AttackStrength;         // 武术
         public BaseInfo                     MagicStrength;          // 灵力
         public BaseInfo                     Defense;                // 防御
         public BaseInfo                     Dexterity;              // 身法（速度）
         public BaseInfo                     FleeRate;               // 吉运（逃逸率）
         public BaseInfo                     PoisonResistance;       // 毒抗
         public ElementalResistanceInfo      ElementResistance;      // 五灵抗性
         public BaseInfo                     Unknown2;               // 未知，待补
         public BaseInfo                     Unknown3;               // 未知，待补
         public BaseInfo                     Unknown4;               // 未知，待补
         public BaseInfo                     CoveredBy;              // 虚弱时会受到谁的援护（PlayerID）
         public MagicInfo                    Magic;                  // 所掌握的仙术
         public BaseInfo                     WalkFrames;             // 行走帧（？？？）
         public BaseInfo                     CooperativeMagic;       // 合体法术
         public BaseInfo                     Unknown5;               // 未知，待补
         public BaseInfo                     Unknown6;               // 未知，待补
         public SoundInfo                    Sound;                  // 各种场合下的音效
      }

      public struct PoisonStatus
      {
         public struct PoisonStatusInfo
         {
            public WORD    wPoisonID;        // 毒性对象编号
            public WORD    wPoisonScript;    // 中毒脚本
         }

         fixed WORD wPoisonStatusInfo[2 * MAX_POISONS * MAX_PLAYABLE_PLAYER_ROLES];

         public PoisonStatusInfo Get(WORD wPoisonStatusID)
         {
            fixed (WORD* lp = wPoisonStatusInfo)
            {
               PoisonStatusInfo* lpPlayer = (PoisonStatusInfo*)lp;

               lpPlayer += wPoisonStatusID;

               return *lpPlayer;
            }
         }
      }

      public struct Inventory
      {
         public struct InventoryInfo
         {
            public WORD       wItem;            // 道具编号
            public USHORT     nCount;           // 现有数量
            public USHORT     nCountInUse;      // 本回合需要消耗的该道具的数量
         }

         fixed WORD wInventoryInfo[3 * MAX_INVENTORY];

         public InventoryInfo Get(WORD wInventoryID)
         {
            fixed (WORD* lp = wInventoryInfo)
            {
               InventoryInfo* lpInventory = (InventoryInfo*)lp;

               lpInventory += wInventoryID;

               return *lpInventory;
            }
         }
      }

      public struct Scene
      {
         public struct SceneInfo
         {
            public WORD    wMapID;                 // 地图编号
            public WORD    wScriptOnEnter;         // 进场脚本
            public WORD    wScriptOnTeleport;      // 传送（土遁）脚本
            public WORD    wEventObjectIndex;      // 此场景的起始对象编号
         }

         fixed WORD wSceneInfo[4 * MAX_SCENES];

         public SceneInfo Get(WORD wSceneID)
         {
            fixed (WORD* lp = wSceneInfo)
            {
               SceneInfo* lpScene = (SceneInfo*)lp;

               lpScene += wSceneID;

               return *lpScene;
            }
         }
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct ObjectInfo
      {
         //
         // 系统对象
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct SystemInfo
         {
            fixed WORD wReserved[6];      // 总是为 0
         }

         //
         // 队员对象
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct PlayerInfo
         {
            fixed    WORD     wReserved[2];              // 总是为 0
            public   WORD     wScriptOnFriendDeath;      // 需我援护者死亡脚本（需吾援者亡，吾甚怒）
            public   WORD     wScriptOnDying;            // 虚弱脚本（吾虚之，援者不屑）
         }

         //
         // 仙术对象
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct MagicInfo
         {
            public   WORD wBaseInfoID;          // 基础信息编号（于 DATA.MKF #3）
                     WORD wReserved1;           // 总是为 0
            public   WORD wScriptOnSuccess;     // 后续脚本（执行条件：施法成功）
            public   WORD wScriptOnUse;         // 前序脚本（此处决定是否施法成功）
                     WORD wReserved2;           // 总是为 0
            public   WORD wMask;                // 枚举属性掩码
         }

         //
         // 道具对象
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct ItemInfo
         {
            public WORD wBitmapID;           // 位图编号（于 BALL.MKF）
            public WORD wPrice;              // 售价（典当则减半）
            public WORD wScriptOnUse;        // 使用脚本
            public WORD wScriptOnEquip;      // 装备脚本
            public WORD wScriptOnThrow;      // 投掷脚本
            public WORD wMask;               // 枚举属性掩码
         }

         //
         // 敌人对象
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct EnemyInfo
         {
            public WORD    wBaseInfoID;               // 基础信息编号（于 DATA.MKF #1）
                                                      // 同时也是位图编号（于 ABC.MKF），二者相对应
            public WORD    wResistanceToSorcery;      // 巫抗（0～10）
            public WORD    wScriptOnTurnStart;        // 开战脚本（多为剧情对话）
            public WORD    wScriptOnBattleEnd;        // 战胜脚本（多为发放战利品）
            public WORD    wScriptOnReady;            // 回合脚本（多为出招表控制）
         }

         //
         // 毒性对象
         //
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct PoisonInfo
         {
            public   WORD     wPoisonLevel;     // 毒的烈度
            public   WORD     wColor;           // 肖像色调
            public   WORD     wPlayerScript;    // 我方中毒脚本（回合结束时执行）
                     WORD     wReserved;        // 总是为 0
            public   WORD     wEnemyScript;     // 敌方中毒脚本（回合结束时执行）
         }

         fixed WORD wObjects[6 * MAX_OBJECTS];

         public PlayerInfo GetPlayer(WORD wObjectID)
         {
            fixed (WORD* lp = wObjects)
            {
               SystemInfo* lpObject;
               PlayerInfo* lpPlayer;

               lpObject = (SystemInfo*)lp;

               lpObject += wObjectID;

               lpPlayer = (PlayerInfo*)lp;

               return *lpPlayer;
            }
         }

         public MagicInfo GetMagic(WORD wObjectID)
         {
            fixed (WORD* lp = wObjects)
            {
               SystemInfo* lpObject;
               MagicInfo* lpMagic;

               lpObject = (SystemInfo*)lp;

               lpObject += wObjectID;

               lpMagic = (MagicInfo*)lpObject;

               return *lpMagic;
            }
         }

         public ItemInfo GetItem(WORD wObjectID)
         {
            fixed (WORD* lp = wObjects)
            {
               SystemInfo* lpObject;
               ItemInfo* lpItem;

               lpObject = (SystemInfo*)lp;

               lpObject += wObjectID;

               lpItem = (ItemInfo*)lpObject;

               return *lpItem;
            }
         }

         public EnemyInfo GetEnemy(WORD wObjectID)
         {
            fixed (WORD* lp = wObjects)
            {
               SystemInfo* lpObject;
               EnemyInfo* lpEnemy;

               lpObject = (SystemInfo*)lp;

               lpObject += wObjectID;

               lpEnemy = (EnemyInfo*)lpObject;

               return *lpEnemy;
            }
         }

         public PoisonInfo GetPoison(WORD wObjectID)
         {
            fixed (WORD* lp = wObjects)
            {
               SystemInfo* lpObject;
               PoisonInfo* lpPoison;

               lpObject = (SystemInfo*)lp;

               lpObject += wObjectID;

               lpPoison = (PoisonInfo*)lpObject;

               return *lpPoison;
            }
         }
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct Event
      {
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
         public struct EventInfo
         {
            public SHORT      sVanishTime;                  // 隐匿帧数（多为怪物事件的隐藏时间）
                                                            // 游戏内每秒 10 帧，乘以 10 即隐藏的秒数
            public WORD       x;                            // 在地图上的 X 轴坐标
            public WORD       y;                            // 在地图上的 Y 轴坐标
            public SHORT      sLayer;                       // 图层 / Y轴偏移
            public WORD       wTriggerScript;               // 触发脚本
            public WORD       wAutoScript;                  // 自动脚本
            public SHORT      sState;                       // 对象状态（【0 隐藏】【1 穿墙/被穿过】【2 实体】【n 其他自定义意义】）
            public WORD       wTriggerMode;                 // 触发方式
                                                            // 【0 禁止触发】
                                                            // 【1 互动，半径为 2，脸贴事件（宝箱）】
                                                            // 【2 互动，半径为 3（对话）】
                                                            // 【3 互动，半径为 4（也是对话，开局先和胖喵对话的 bug）】
                                                            // 【4 接触，半径为 1，与事件重合（将军冢活地板机关）】
                                                            // 【5 接触，半径为 2（部分场景切换点）】
                                                            // 【n 接触，其他自定义半径】
            public WORD       wSpriteID;                    // 场景形象编号
            public USHORT     nDirectionFrames;             // 每个方向有多少帧（一般为 3）
            public WORD       wDirection;                   // 面朝方向（第几组帧序列）
            public WORD       wCurrentFrameID;              // 当前帧编号
            public USHORT     nScriptIdleFrame;             // 触发脚本指令（多为 0x0002 0x0003）累计执行帧所用
            public WORD       wSpritePtrOffset;             // 未知，待补
            public USHORT     nSpriteFramesAuto;            // 总帧数，程序自动计算得出（由只在内存中有意义）
            public WORD       wScriptIdleFrameCountAuto;    // 自动脚本指令（多为 0x0002 0x0003 0x0009）累计执行帧所用
         }

         fixed WORD wEventObject[16 * 5332];
      }  

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct GameSave
      {
         public         WORD              wSavedTimes;               // 存盘次数
         public         WORD              wViewportX, wViewportY;    // 视口 坐标
         public         WORD              nPartyMember;              // 队员人数
         public         WORD              wSceneID;                  // 场景 ID
         public         WORD              wPaletteOffset;            // 调色板偏移（暂时用不到．．．）
         public         WORD              wPartyDirection;           // 队伍方向
         public         WORD              wMusicID;                  // 音乐 ID
         public         WORD              wBattleMusicID;            // 战斗音乐编号
         public         WORD              wBattleFieldID;            // 战斗环境/背景图编号
         public         WORD              wScreenWave;               // 屏幕波浪效果程度（赤鬼王血池，尚书府，七星磐龙柱，水下）
                        WORD              wReserved1;                // 未使用，无效
         public         WORD              wCollectValue;             // value of "collected" items
         public         WORD              wLayer;                    // 队伍的所在图层
         public         WORD              wChaseRange;               // 怪物追逐敏感等级（【0 驱魔香】【1 正常】【2 十里香】【n 自定义敏感等级】）
         public         WORD              wChasespeedChangeCycles;   // 怪物追逐敏感时间（驱魔香/十里香时间），结束后变回 1（正常）
         public         WORD              nFollower;                 // 跟随队伍的 NPC 数量（原版此值加上队员人数不得大于 5）
                  fixed WORD              wReserved2[3];             // 未使用，无效
         public         DWORD             dwCash;                    // 现有金钱数
         public         Party             Party;                     // 队伍信息
         public         Trail             Trail;                     // 行走路线记录
         public         Exp               ExpErience;                // 各种属性的隐藏经验值
         public         PlayerRole        PlayerRoles;               // 队员基础数据
         public         PoisonStatus      PoisonStatus;              // 队员中毒信息
         public         Inventory         Inventory;                 // 库存道具
         public         Scene             Scene;                     // 所有场景数据
         public         ObjectInfo        Object;                    // 所有对象信息
         public         Event             EventObject;               // 所有事件数据
      }

      public struct GlobalVar
      {
         public Files         files;            // 游戏文件的字节数据
         public LPSTR[]       messages;         // 所有对话
         public LPSTR[]       obgect_name;      // 所有对象的名称
         public Palette[]     palette;          // 调色板集合

         public WORD          wPaletteID;       // 当前调色板 ID
         public BOOL          fNightPalette;    // 是否为夜间调色板

         public BYTE          bCurrentSaveSlot; // 当前存档 ID
         public GameSave      save;             // 全局存档数据

         public BYTE*         FONT_ICON;
      }

      public static GlobalVar g_Global;

      private static void
      InitFileData()
      /*++
       * 
       * 作用：
       *    初始化游戏资源文件的数据
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT         i;
         BYTE[]      dataBits;

         //
         // 读取游戏文件字节数据
         //
         fixed (Files* fpFileData = &g_Global.files)
         {
            BYTE** lpFileData = (BYTE**)fpFileData;

            for (i = 0; i < GAME_FILE_NAME_LIST.Length; i++)
            {
               dataBits       = FILE.ReadAllBytes($"{g_lpszGamePath}/{GAME_FILE_NAME_LIST[i]}");
               lpFileData[i]  = (BYTE*)Marshal.AllocHGlobal(dataBits.Length);
               Marshal.Copy(dataBits, 0, (IntPtr)lpFileData[i], dataBits.Length);
            }
         }
      }

      public static void
      InitMessges()
      /*++
       * 
       * 作用：
       *    初始化游戏中的对象名称和角色对白
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DWORD       dwLen, nMsg, dwThisMsgID, dwThisMsgAddr, dwNextMsgAddr;
         BYTE[]      bytes, thisBytes;
         DWORD*      lpFileData;
         BYTE*       lpSSS;
         LPSTR       lpszThisMsg;

         //
         // 注册 .NET 内码包
         //
         Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

         //
         // 读取 <对话> 文件字节
         //
         bytes = FILE.ReadAllBytes($"{g_lpszGamePath}/{GAME_FILE_MESSAGES_NAME}");

         //
         // 分割 <对话>
         //
         lpSSS = g_Global.files.SSS_MKF;
         {
            //
            // 获取所有 <对话> 索引
            //
            {
               lpFileData = (DWORD*)lpSSS;

               //
               // 获取 <SSS_SUB_3> 的长度
               //
               dwLen = lpFileData[4] - lpFileData[3];

               //
               // 获取 <对话> 计数
               //
               nMsg = dwLen / sizeof(DWORD) - 1;

               //
               // 将指针定位到 <SSS_SUB_3> 的开头
               //
               lpFileData = (DWORD*)&lpSSS[lpFileData[3]];
            }

            //
            // 初始化 <对话列表>
            //
            g_Global.messages = new LPSTR[nMsg];

            //
            // 根据 <对话索引> 分割对话
            //
            for (dwThisMsgID = 0; dwThisMsgID < nMsg; dwThisMsgID++)
            {
               //
               // 获取单句 <对话索引>
               //
               dwThisMsgAddr = lpFileData[dwThisMsgID];
               dwNextMsgAddr = lpFileData[dwThisMsgID + 1];

               //
               // 分割单句 <对话>
               //
               thisBytes = new BYTE[dwNextMsgAddr - dwThisMsgAddr];
               Array.Copy(bytes, dwThisMsgAddr, thisBytes, 0, thisBytes.Length);
               lpszThisMsg = Encoding.GetEncoding("GBK").GetString(thisBytes);

               //
               // 添加到 <对话列表>
               //
               g_Global.messages[dwThisMsgID] = lpszThisMsg;
            }
         }

         //
         // 读取 <对象名称> 文件字节
         //
         bytes = FILE.ReadAllBytes($"{g_lpszGamePath}/{GAME_FILE_OBJECTNAME_NAME}");

         //
         // 分割 <对象名称>
         //
         {
            //
            // 初始化 <对象名称列表>
            //
            g_Global.obgect_name = new LPSTR[nMsg];

            //
            // 获取 <对象名称> 计数
            //
            nMsg = (DWORD)(bytes.Length / OBJECTNAME_SUB_LEN);

            //
            // 分割 <对象名称>，每对象名称长度为 10
            //
            for (dwThisMsgID = 0; dwThisMsgID < nMsg; dwThisMsgID++)
            {
               //
               // 获取单个 <对象名称> 的索引
               //
               dwThisMsgAddr = dwThisMsgID * OBJECTNAME_SUB_LEN;

               //
               // 分割单句对话
               //
               thisBytes = new BYTE[OBJECTNAME_SUB_LEN];
               Array.Copy(bytes, dwThisMsgAddr, thisBytes, 0, thisBytes.Length);
               lpszThisMsg = Encoding.GetEncoding("GBK").GetString(thisBytes);

               //
               // 添加到对话列表
               //
               g_Global.obgect_name[dwThisMsgID] = lpszThisMsg.TrimEnd();
            }
         }
      }

      public static void
      Init()
      /*++
       * 
       * 作用：
       *    初始化全局游戏数据
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         //
         // 初始化游戏文件的字节数据
         //
         InitFileData();

         //
         // 初始化游戏内对话和对象名称
         //
         InitMessges();

         //
         // 初始化调色板
         //
         PalPalette.Init();

         //
         // 初始化对话末尾光标
         //
         PalText.InitFontIcon();

         //
         // 初始化 UI 界面
         //
         PalUi.Init();
      }

      public static void
      Free()
      /*++
       * 
       * 作用：
       *    初始化游戏资源文件的数据
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT         i;

         //
         // 释放游戏文件字节数据
         //
         fixed (Files* fpFileData = &g_Global.files)
         {
            BYTE** lpFileData = (BYTE**)fpFileData;

            for (i = 0; i < GAME_FILE_NAME_LIST.Length; i++)
            {
               Marshal.FreeHGlobal((IntPtr)lpFileData[i]);
               lpFileData[i] = null;
            }
         }

         //
         // 释放对话末尾光标资源
         //
         PalText.FreeFontIcon();

         //
         // 释放 UI 界面资源
         //
         PalUi.Init();
      }

      public static WORD
      GetSavedTimes(
         INT      iSaveSlot
      )
      {
         WORD     wSavedTimes;
         LPSTR    lpszSavePath;
         BYTE[]   buffer;

         wSavedTimes    = 0;
         lpszSavePath   = $@"{g_lpszGamePath}\{iSaveSlot}.rpg";
         buffer         = new BYTE[2];

         if (FILE.Exists(lpszSavePath))
         {
            using (FileStream fs = FILE.OpenRead(lpszSavePath))
            {
               fs.Read(buffer, 0, 2);

               fixed (BYTE* bytes = buffer)
               {
                  wSavedTimes = ((WORD*)bytes)[0];
               }
            }
         }

         return wSavedTimes;
      }

      public static void
      ReadGameSave(
         INT      iGameSaveID
      )
      {

      }
   }
}
