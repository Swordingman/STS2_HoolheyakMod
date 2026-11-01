# 霍尔海雅卡牌检查报告（2026-10-31）

已检查97张实际卡牌类，对照当前描述审阅实现，并执行未升级、升级及常规变量分支的原生结算测试。确定的问题已修复；稳定版和测试版最终构建均为0警告、0错误。以下通过结论限于所列测试范围，不代表已完成真实游戏界面、联网或存读档验收。

## 备份与改动范围

修改前备份：[before-audit.zip](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/before-audit.zip)（286,550字节）。备份含本轮开始时的源码、项目文件和本地化；不是包含图片的完整项目镜像。压缩包中的 `HoolheyakModCore/...` 对应项目同路径，`localization/...` 对应项目的 `HoolheyakMod/localization/...`。恢复会覆盖本轮修改，后续改动也需单独保留。

本轮相对备份修改34个已有C#文件及中英4个本地化JSON文件，新增业务辅助类 [TypedDraw.cs](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Scripts/TypedDraw.cs)。没有修改卡图。测试工程、检查器、日志和运行时均保存在项目外：[完整测试目录](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030)，不加入模组编译。项目中新增本报告。

## 最终验证结果

| 分支 | 中文难度0 | 英文难度0 | 中文难度1 | 中文难度2 | 专项交互 | 构建 |
| --- | --- | --- | --- | --- | --- | --- |
| Stable | 240场景/371断言 | 240/371 | 240/371 | 240/371 | 38场景/124断言 | 0警告、0错误 |
| Beta | 240场景/371断言 | 240/371 | 240/371 | 240/371 | 38场景/124断言 | 0警告、0错误 |

上表全部通过。三个难度是模组配置的0、1、2；专项交互测试使用中文难度0。合计1,996次场景执行、3,216次断言，包含不同语言、难度和版本的重复覆盖。每轮基础测试的240个场景覆盖同一套97张卡牌。

中英本地化静态检查覆盖97张卡牌、45个能力、17个遗物和3个药水，未发现检查范围内的缺失键；另行核对选牌提示和变量选项键，未发现缺失。新增源码行未发现行尾空白。

## 已修复的问题

1. **定向抽牌与手牌选择**：动压现在抽攻击牌；脚注使用原生抽牌、洗牌、抽牌记录及限制处理；交叉检索改为让玩家选择弃牌，并补上中英选择提示。
2. **攻击分组**：斜压不稳、激波使用单次群体攻击；逆风、浸润性坏死、坏疽剥离、双生子实验使用原生多段攻击。修复下一次攻击加成只覆盖部分目标/段数的问题，逆风仍在每一段伤害后施加升力。
3. **重力、浮空、摔落**：按用户确认改为最大生命10%向下取整，对冲再向下取整减半，允许重力为0；达到条件时清空已有升力，再进入浮空。摔落不再被自身浮空的1.5倍承伤重复放大。全知视界改为触发实际持有者的抽牌和能量收益。
4. **相位与归属**：六合的升力限制钩子存在两个同类型参数位置颠倒，现已修复。对冲的持有者半格挡、队友格挡不受影响，以及六合/拱相等相关交互在双角色隔离场景中通过。真实联网同步未测试。
5. **减费与消耗**：对流补上消耗且只统计自己的攻击；撰写的减费持续整回合；曾有羽翼保持“下次打出或本回合结束”的期限，并补全中英描述。
6. **普通版/升级版叠层**：副产品、羽蛇的衣柜分别记录升级来源层数，生成牌的升级/0费效果只作用于对应数量，避免一层升级能力强化所有普通层。能力说明同步显示对应数量。
7. **持续能力触发**：修正变量忽略队友抽到的状态/诅咒；传承终焉连续交替攻击与技能时不再漏奖励；新陈律动正确排队处理嵌套新增状态；孤星映盈按整回合自己的攻击牌伤害统计，包含打出能力前的攻击，排除博览等非攻击牌伤害，且多层按层数触发。
8. **递归实验**：首次只支付卡牌费用，后续每继续一轮额外支付1能量，停止不另收费。使用循环处理选择，在自动打出可能结束战斗前登记奖励，并保护已被原生流程移除的能力牌。状态/诅咒沿用既有“实验失败的产物”计数规则，并补全描述；抽到不可打出的状态也能正确移除。
9. **冗余实验**：选牌数量与描述一致；标记保留整回合，同一张牌多次被弃置仍可触发，自动打出增加重入保护，回合结束清除。
10. **蜕生条件**：双生病程、代偿增生、空骨症候、寄枝返生、坏疽剥离在效果开始时记录蜕生条件，避免结算过程中状态变化导致判断漂移。双生病程的自动变量触发也必须满足蜕生。
11. **其他描述一致性**：驳论能处理一般正面增益，排除基础重力；文献检索不再把自己的颜色排除出随机颜色池；库库尔坎的传承明确翻倍的是博览/逶迤的伤害与升力；万界测绘避免重复移除已经自行退出战斗的生成攻击牌。

## 专项测试覆盖

除逐卡基础测试外，以下每一项均在Stable和Beta执行；名称对应测试日志。

- typed draw preserves draw hooks/order and restrictions
- footnote reshuffles and uses native draw
- hand choice is player-selected and not first-card discard
- compose reduction survives replay but expires at turn end
- convection exhaust and owner-only cost history
- corrected variable ignores teammate draws
- inherited end consecutive alternating sequence
- gravity rounding, zero gravity, lift cleared and horizon owner
- fall damage and coercion immediate payoff
- deconstruction consumes actual block and ecdysis cleanup
- flyup captures Analysis across memories triggers
- upgraded/non-upgraded byproduct stacks remain distinct
- upgraded/non-upgraded wardrobe stacks remain distinct
- renewal nested new state counted once and later stacks ignored
- native multi-hit and AOE preserve next attack vigor
- canonical replay and next skill replay use native card loop
- parameter correction executes native end-hand wrapper
- slough conditions and both twinned pathology branches
- necrotic removal triggers once per cleared debuff
- exoskeleton replay snapshots slough condition
- foreign-color discount duration is next play or turn end
- entropy and forgetfulness turn effects
- distilled time potion and doom upkeep
- serpent ritual follows Analysis increments
- voyage counts earlier attack damage but excludes proc damage
- opposition owner block and teammate isolation
- sextile lift restriction and trine attack exhaust
- quincunx and variable tags align with triggers
- refutation strips non-stat buffs and excludes debuffs
- recursive first experiment with no remaining energy
- recursive experiment removes an unplayable status
- recursive experiment handles a power card
- redundant marking lasts for the entire turn
- slough condition survives deconstruction being consumed by block
- reproducibility grows once per full replay series
- wake previous-card check in native play loop
- native gold glow follows shared owner power conditions
- All-In executes both effects but variable hooks once

## 逐卡结果

每行均已执行未升级、升级和该牌常规变量分支。效果列来自当前中文难度0未升级牌的原生格式化文本，为阅读方便把能量图标转为数字，并省略测试环境自动追加的英文关键词标签；相关关键词另从模型检查。表中费用依次为未升级/升级。X费基础用例以已支付X=2作为输入。通过表示基础结算通过，额外说明列标注专项覆盖或限制。

| 卡牌 | 费用 | 未升级效果 | 结果与说明 |
| --- | --- | --- | --- |
| [古老盟约](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/AncientCovenant.cs) | 1 / 1 | 变量：<br>触发博览时，获得1力量；<br>触发逶迤时，获得1敏捷。 | 基础结算通过。 |
| [凋亡实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ApoptosisExperiment.cs) | 1 / 1 | 造成12点伤害。<br>给予目标变量：<br>4层升力；<br>15层解构。 | 基础结算通过。 |
| [归档](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Archive.cs) | 1 / 1 | 造成10点伤害。<br>斩杀时，在战斗结束后额外选择1张变量：<br>随机颜色牌；<br>无色牌。 | 非斩杀基础路径通过；斩杀后的真实奖励界面仍需游戏内验证。 |
| [扩增实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/AugmentationExperiment.cs) | 1 / 1 | 造成5点伤害。<br>变量：<br>获得1层解析；<br>将一张新的扩增实验置入手牌，并获得3层解析。 | 基础结算通过。 |
| [斜压不稳](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/BaroclinicInstability.cs) | 1 / 1 | 对所有敌人造成6点伤害，<br>给予4层解构，<br>并给予1层升力。 | 本轮已修复，基础结算通过。 |
| [借用武装](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/BorrowedWeapons.cs) | 1 / 1 | 消耗。<br>造成9点伤害。<br>随机将1张其他颜色的攻击牌加入你的手牌。 | 基础结算通过。 |
| [笔锋](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Brushstroke.cs) | 0 / 0 | 造成4点伤害。<br>获得1博览。 | 基础结算通过。 |
| [曾有羽翼](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/BygoneWings.cs) | 2 / 1 | 每当你打出1张其他颜色的牌，使1张随机手牌耗能-1，直到它下次打出或本回合结束。 | 已补全减费期限，并验证下次打出/回合结束失效。 |
| [副产品](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Byproduct.cs) | 1 / 1 | 每当你进行一次变量，获得1张随机牌。 | 已修复普通版和升级版叠层混用。 |
| [嵌合体实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ChimeraExperiment.cs) | X / X | 打出你的抽牌堆顶端的X张变量：<br>攻击牌；<br>技能牌。 | 基础结算通过。 |
| [强压](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Coercion.cs) | 2 / 2 | 造成12点伤害。<br>若目标浮空，使其摔落并获得相当于此次摔落伤害的格挡，以及1点能量。 | 基础结算通过。 |
| [冷眼](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ColdEye.cs) | 1 / 1 | 如果目标在本回合中受到过来自你的伤害，则获得3点解析；如果没有，则获得12点格挡。 | 基础结算通过。 |
| [共病模型](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ComorbidityModel.cs) | 2 / 2 | 造成13点伤害。<br>蜕生：你每拥有1种不同的负面状态，额外造成2点伤害。 | 基础结算通过。 |
| [代偿增生](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/CompensatoryHyperplasia.cs) | 1 / 1 | 抽3张牌，消耗1张手牌。<br>蜕生：然后再抽1张牌。 | 本轮已修复，基础结算通过。 |
| [互补实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ComplementaryExperiment.cs) | 1 / 1 | 每当你进行一次变量时，获得2点格挡。 | 基础结算通过。 |
| [撰写](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Compose.cs) | 1 / 0 | 直到回合结束，变量：<br>使一张手牌耗能-3；<br>使所有手牌耗能-1。 | 已修复本回合减费被一次打出提前清除。 |
| [预案](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ContingencyPlan.cs) | 0 / 0 | 获得1点能量。<br>每当触发逶迤时，将此牌从弃牌堆移回手牌。 | 基础结算通过。 |
| [对照实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ControlExperiment.cs) | 1 / 1 | 造成9点伤害。<br>变量：<br>将1张随机相位置入你的抽牌堆，并抽2张牌；<br>将1张随机相位置入你的弃牌堆，随后立即洗牌。 | 基础结算通过。 |
| [对照组](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ControlGroup.cs) | 1 / 1 | 造成6点伤害。<br>给予1层失重。<br>变量：<br>攻击卡；<br>技能卡。 | 基础结算通过。 |
| [对流](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Convection.cs) | 4 / 3 | 消耗。<br>造成18点伤害。<br>本回合每打出1张攻击牌，耗能-1。 | 已补消耗，减费只统计自己的攻击。 |
| [气流辐合](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Convergence.cs) | 1 / 1 | 消耗。<br>获得8点格挡。<br>额外获得所有敌人重力总和的格挡（最多15点）。 | 基础结算通过。 |
| [修正变量](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/CorrectedVariable.cs) | 1 / 1 | 每当你抽到一张状态牌，获得1点博览。<br>每当你抽到一张诅咒牌，获得1点逶迤。 | 已修复队友抽牌误触发。 |
| [交叉实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/CrossExperiment.cs) | 1 / 1 | 造成4点伤害。<br>获得4层格挡。<br>打出时，该卡永久获得变量：<br>+1伤害；<br>+1格挡。 | 基础结算通过。 |
| [交叉检索](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/CrossSearch.cs) | 0 / 0 | 抽2张牌，弃1张牌。<br>若弃置攻击牌，获得1点博览；<br>若弃置技能牌，获得1点逶迤。 | 已改为实际选择手牌弃置。 |
| [气旋](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Cyclone.cs) | 1 / 1 | 造成6点伤害。<br>获得1点博览。<br>获得1层解析。 | 基础结算通过。 |
| [深思](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Deepthink.cs) | 2 / 2 | 获得2点解析。<br>获得2层逶迤和博览。 | 基础结算通过。 |
| [防御](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/DefendHoolheyak.cs) | 1 / 1 | 获得5点格挡。 | 基础结算通过。 |
| [岁月蒸馏](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/DistilledTime.cs) | 3 / 3 | 立刻从攻击药水、技能药水和能力药水中随机获得1瓶。<br>在你的回合开始时，获得3层灾厄并再获得1瓶。 | 基础结算通过。 |
| [气流辐散](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Divergence.cs) | 1 / 1 | 消耗。<br>获得12点格挡。<br>消耗目标所有升力，每消耗1层，额外获得2点格挡。 | 基础结算通过。 |
| [动压](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/DynamicPressure.cs) | 1 / 1 | 造成5点伤害。<br>抽2张攻击牌。 | 已修复为定向抽攻击牌。 |
| [蜕鳞期](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Ecdysis.cs) | 2 / 1 | 每当你解除1种负面状态，获得1点解析和变量：<br>1点力量；<br>1点敏捷。 | 基础结算通过。 |
| [熵增](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/EntropyIncrease.cs) | 2 / 1 | 在回合开始时，增加1张随机颜色的能力牌到你的手牌。 | 基础结算通过。 |
| [析因实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/FactorialExperiment.cs) | 2 / 2 | 造成12点伤害。<br>变量：<br>触发手牌中（大部分）变量卡的随机变量；<br>将3张随机变量卡置入你的手牌，并在本回合中使它们的耗能变为0。 | 基础结算通过。 |
| [假性愈合](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/FalseHealing.cs) | 1 / 1 | 获得10点格挡。<br>获得1层脆弱。 | 基础结算通过。 |
| [羽击](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/FeatherStrike.cs) | 0 / 0 | 造成3点伤害。<br>给予1层升力。 | 基础结算通过。 |
| [飞起来！](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/FlyUp.cs) | X / X | 获得1点解析。<br>触发X次变量：<br>博览；<br>逶迤。<br>本次触发使用开始时的解析值。 | 基础结算通过。 |
| [脚注](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Footnote.cs) | 1 / 1 | 获得8点格挡。<br>抽1张技能牌。 | 已修复为原生抽牌流程，包含洗牌和抽牌限制。 |
| [禁忌知识](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ForbiddenKnowledge.cs) | 1 / 1 | 变量：<br>触发博览时，额外抽1张牌；<br>触发逶迤时，对所有敌人造成5点伤害。 | 基础结算通过。 |
| [伏笔](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Foreshadowing.cs) | 2 / 2 | 获得10点格挡。<br>获得2点博览。<br>获得1点逶迤。 | 基础结算通过。 |
| [忘却](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Forgetfulness.cs) | 2 / 2 | 你的回合开始时，从3张随机牌中选择1张加入手牌。它在本回合可以免费打出，打出后移出本场战斗。 | 基础结算通过。 |
| [但为求索](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ForTheQuest.cs) | 0 / 0 | 造成8点伤害。<br>给予目标相当于其重力一半的升力（向下取整）。 | 基础结算通过。 |
| [逆风](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Headwind.cs) | 1 / 1 | 对随机敌人造成2点伤害3次。<br>每次造成伤害，给予一层升力。 | 已修复多段攻击分组，逐段给予升力。 |
| [空骨症候](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/HollowboneSyndrome.cs) | 1 / 1 | 造成9点伤害。给予目标2层失重。<br>蜕生：给予目标2层升力。 | 本轮已修复，基础结算通过。 |
| [浸润性坏死](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/InfiltrativeNecrosis.cs) | 2 / 2 | 造成1点伤害12次。<br>然后获得99层黑暗镣铐。 | 本轮已修复，基础结算通过。 |
| [传承终焉](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/InheritedEnd.cs) | 1 / 0 | 消耗。<br>本回合中，每当交替打出攻击牌与技能牌时，获得1点能量并抽1张牌。 | 已修复连续交替出牌漏奖励。 |
| [传承记忆](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/InheritedMemories.cs) | 1 / 1 | 你每触发一次逶迤或者博览，就获得1点解析。 | 基础结算通过。 |
| [敲入实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/KnockInExperiment.cs) | 1 / 1 | 造成7点伤害。<br>变量：<br>花费1点能量，回复3生命；<br>获得1点能量。 | 基础结算通过。 |
| [库库尔坎的传承](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/KukulkanLegacy.cs) | 3 / 2 | 使你的博览和逶迤触发所需的层数各+2，其造成的伤害和给予的升力翻倍。 | 已明确翻倍作用于博览/逶迤效果。 |
| [层流](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/LaminarFlow.cs) | 0 / 0 | 造成3点伤害。<br>抽1张牌。 | 基础结算通过。 |
| [盘亘](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Linger.cs) | 1 / 1 | 获得6点格挡。<br>查看牌堆顶的1张牌，<br>选择至多1张卡，变量：<br>弃掉它们；<br>将它们置于抽牌堆底部。<br>抽1张牌。 | 基础结算通过。 |
| [文献检索](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/LiteratureRetrieval.cs) | 1 / 1 | 从3张随机颜色的牌中选择1张加入你的手牌。 | 已修复随机颜色牌池排除自身颜色。 |
| [博览者的狂语](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/MadWords.cs) | 3 / 3 | 造成6点伤害，给予2层升力。<br>重放2。 | 基础结算通过。 |
| [质量抵消](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/MassOffset.cs) | X / X | 使所有敌人获得X点升力，并获得X点解析。 | 基础结算通过。 |
| [化生皮层](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/MetaplasticSkin.cs) | 1 / 1 | 每当你打出1张牌时，蜕生：根据牌的类型获得1层博览或逶迤。 | 基础结算通过。 |
| [诱变实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/MutagenesisExperiment.cs) | 1 / 1 | 造成12点伤害。<br>变量：<br>随机触发一个相位；<br>消耗1点能量，随机触发一个吉相。 | 基础结算通过。 |
| [坏疽剥离](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/NecroticStripping.cs) | 3 / 3 | 造成5点伤害3次。<br>蜕生：清除你身上的所有负面状态。每清除1种，触发1次博览。 | 本轮已修复，基础结算通过。 |
| [观测](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Observation.cs) | 1 / 0 | 抽1张牌。<br>获得1点能量。 | 基础结算通过。 |
| [全知视界](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/OmniscientHorizon.cs) | 2 / 1 | 每当有敌人进入浮空状态，抽2张牌，获得2点能量。 | 已修复浮空时寻找错误持有者。 |
| [参数修正](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ParameterCorrection.cs) | 0 / 0 | 消耗。<br>回合结束时，若此牌在你手中，可以至多使1张手牌在本回合中获得保留。<br>每次触发该效果，获得1层逶迤。 | 原生回合末手牌回调、保留选择及逶迤收益通过。 |
| [寄枝返生](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ParasiticRegrowth.cs) | 1 / 1 | 从3张随机相同颜色的牌中选择1张加入手牌。<br>蜕生：使其在本回合中获得重放1。 | 本轮已修复，基础结算通过。 |
| [合相](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Phases/ConjunctionCard.cs) | 不可打出 / 不可打出 | 中性<br>不可打出。<br>抽到时自动触发并消耗。<br>替换你的相位为合相：你造成的伤害，以及敌人对你造成的伤害增加50%。 | 基础结算通过。 |
| [对冲](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Phases/OppositionCard.cs) | 不可打出 / 不可打出 | 凶相<br>不可打出。<br>抽到时自动触发并消耗。<br>替换你的相位为对冲：你获得的格挡减半。敌人的重力视为原本的一半。 | 基础结算通过。 |
| [梅花相](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Phases/QuincunxCard.cs) | 不可打出 / 不可打出 | 中性<br>不可打出。<br>抽到时自动触发并消耗。<br>替换你的相位为梅花相：你的技能牌提供博览，攻击牌提供逶迤。 | 基础结算通过。 |
| [六合](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Phases/SextileCard.cs) | 不可打出 / 不可打出 | 吉相<br>不可打出。<br>抽到时自动触发并消耗。<br>替换你的相位为六合：你的博览与逶迤触发需要的层数减少2点，但你无法再给予升力。 | 已修复钩子同类型参数位置颠倒导致升力限制失效。 |
| [刑克](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Phases/SquareCard.cs) | 不可打出 / 不可打出 | 凶相<br>不可打出。<br>抽到时自动触发并消耗。<br>替换你的相位为刑克：你每打出一张牌，失去1点生命并给予所有敌人1层升力。 | 基础结算通过。 |
| [拱相](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Phases/TrineCard.cs) | 不可打出 / 不可打出 | 吉相<br>不可打出。<br>抽到时自动触发并消耗。<br>替换你的相位为拱相：进入时及你的回合开始时获得1点能量并抽2张牌。你的攻击牌附带消耗。 | 基础结算通过。 |
| [动力外骨骼](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/PoweredExoskeleton.cs) | 1 / 1 | 获得3层覆甲。<br>蜕生：重放1次。 | 基础结算通过。 |
| [朽壤孳生](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/PutrescentGermination.cs) | 1 / 1 | 消耗1张手牌。<br>使你身上的1种随机负面状态各减少1层。 | 基础结算通过。 |
| [腐秽祭仪](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/PutridOblation.cs) | 1 / 1 | 本回合变量：<br>失去所有格挡且无法获得格挡，抽3张牌并获得3点能量；<br>无法抽牌，获得15点格挡和3点能量；<br>无法获得能量，获得15点格挡并抽3张牌。 | 基础结算通过。 |
| [急流](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Rapids.cs) | 1 / 0 | 消耗。<br>给予99层失重。 | 基础结算通过。 |
| [返生反射](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/RebirthReflex.cs) | 1 / 1 | 造成6点伤害。<br>使你在本回合中打出的下一张技能牌获得重放1。 | 基础结算通过。 |
| [递归实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/RecursiveExperiment.cs) | 1 / 0 | 首次实验无需额外耗能，之后每轮实验额外花费1点能量。<br>变量：<br>终止实验；<br>随机打出1张牌并将其从卡组中永久移除。战斗后，额外获得一组同类型选牌奖励，然后继续实验。若为诅咒或状态，则改为使你拥有的“实验失败的产物”计数+1。 | 已修复收费、循环、奖励登记时机及能力牌重复移除；补全诅咒/状态例外。 |
| [冗余实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/RedundantExperiment.cs) | 2 / 2 | 选择手牌中的1张牌，变量：<br>在本回合中，被选中的牌在被丢弃时视为被打出。（回合结束的弃牌也会触发！）；<br>弃掉它们，并抽等量的牌。 | 已修复选牌数量和整回合标记持续时间。 |
| [驳论](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Refutation.cs) | 1 / 1 | 造成14点伤害。<br>随机使目标身上的增益效果减少1层。 | 已修复移除增益仅支持力量/敏捷的问题。 |
| [新陈律动](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/RenewalRhythm.cs) | 3 / 2 | 每当你以非叠层的方式获得1种新的状态，抽1张牌，并获得1点能量。 | 已修复嵌套新增状态漏触发、后续叠层误触发。 |
| [复现性危机](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/ReproducibilityCrisis.cs) | 2 / 2 | 当你打出一张重放牌后，使它的重放次数+1。 | 基础结算通过。 |
| [考据](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Research.cs) | 1 / 1 | 抽2张牌。<br>获得1点逶迤。 | 基础结算通过。 |
| [修订](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Revision.cs) | 1 / 1 | 消耗。<br>消耗你手中的一张牌，并随机获得一张与其同类型的牌。 | 基础结算通过。 |
| [分离](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Separation.cs) | 1 / 1 | 造成14点伤害。<br>扣除目标等量的最大生命值。 | 基础结算通过。 |
| [羽蛇血脉](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/SerpentBloodline.cs) | 3 / 2 | 获得等同于解析层数的仪式。然后每次获得解析时，获得等量仪式。<br>每回合开始时，失去1点能量。 | 基础结算通过。 |
| [激波](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Shockwave.cs) | 1 / 1 | 对所有敌人造成12点伤害，并给予3层失重。 | 本轮已修复，基础结算通过。 |
| [观星](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Stargazing.cs) | 0 / 0 | 查看牌堆顶的3张牌，<br>选择至多3张卡，变量：<br>弃掉它们；<br>将它们置于抽牌堆底部。 | 基础结算通过。 |
| [打击](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/StrikeHoolheyak.cs) | 1 / 1 | 造成6点伤害。 | 基础结算通过。 |
| [测绘](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Surveying.cs) | 1 / 1 | 造成8点伤害。<br>获得1解析。 | 基础结算通过。 |
| [尾刺](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/TailSting.cs) | 1 / 1 | 造成8点伤害。<br>给予1层易伤。 | 基础结算通过。 |
| [篡改](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Tamper.cs) | 1 / 1 | 获得8点格挡。<br>如果本牌触发了逶迤，额外打出一次。 | 基础结算通过。 |
| [行运](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Transit.cs) | 2 / 1 | 消耗。<br>选择并触发一个相位。 | 基础结算通过。 |
| [过渡流](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/TransitionalFlow.cs) | 1 / 1 | 获得4点格挡。<br>重放1。 | 基础结算通过。 |
| [湍流](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Turbulence.cs) | 0 / 0 | 造成3点伤害。<br>每回合限1次，本场战斗中每打出2张湍流，将1张湍流加入手牌。 | 基础结算通过。 |
| [双生子实验](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/TwinExperiment.cs) | 1 / 1 | 对随机两个敌人造成5点伤害2次。<br>若击中两个不同的敌人，则将额外的2次5点伤害给予变量：<br>剩余血量更多的敌人；<br>剩余血量更少的敌人。 | 本轮已修复，基础结算通过。 |
| [双生病程](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/TwinnedPathologies.cs) | 1 / 1 | 获得8点格挡。<br>蜕生：随机使你身上的1种负面状态变量：<br>减少1层，并抽1张牌；<br>增加1层，并获得1点解析。 | 已修复格挡消耗最后一层解构后漏蜕生，以及自动触发绕过条件。 |
| [万界测绘](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/UniversalMapping.cs) | 3 / 3 | 造成6点伤害。随机打出3张其他颜色的稀有攻击牌。它们打出后移出本场战斗。 | 已补已移出牌的保护；额外攻击结算后移出战斗通过。 |
| [孤星映盈](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/VoyageOfLoneTrail.cs) | 3 / 2 | 回合结束时，若你本回合造成的攻击伤害低于6，触发逶迤与博览。 | 已修复统计窗口、攻击伤害范围及多层触发次数。 |
| [尾流](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Wake.cs) | 1 / 1 | 获得5点格挡。<br>获得1层解析。<br>如果你打出的上一张牌是技能牌，本牌额外打出一次。 | 基础结算通过。 |
| [羽蛇的衣柜](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Wardrobe.cs) | 2 / 1 | 每打出1张其他颜色的牌，随机将1张本颜色牌加入手牌并赋予消耗。 | 已修复普通版和升级版叠层混用。 |
| [切变](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/WindShear.cs) | 1 / 1 | 造成5点伤害。<br>重放1。 | 基础结算通过。 |
| [翼刃](D:/Projects/SlayTheSpireMods/sts2/STS2_Hoolheyak/HoolheyakModCore/Cards/Wingblade.cs) | 1 / 1 | 造成9点伤害。<br>如果本牌触发了博览，额外打出一次。 | 基础结算通过。 |

## 测试方法与尚未覆盖的部分

基础用例直接执行卡牌实际的 `OnPlay`，使用游戏原生伤害、格挡、抽牌和能力指令，并核对生命、能量、牌堆、能力层数和选择次数。卡牌费用读取模型配置；基础用例不模拟玩家手动支付费用。重放、生成牌移出、部分期限和回合末选择等另通过原生自动打出/回调流程专项验证，因此不能把全部基础用例视作完整的一局游戏。

隔离环境提供固定战斗、牌堆和随机种子，用选择器代替玩家点选；对日志、引擎时钟、测试模式下的洗牌帧时间等做边界适配。药水库存入库使用测试适配，未验证真实存档写入。只修改外部测试目录里的游戏DLL私有副本，不修改游戏安装目录或官方参考DLL。

尚需在游戏内观察：真实奖励页面与其他页面的竞争（尤其归档斩杀普通敌人、事件敌人后的奖励）、真实联机同步、存读档、全部动画/黑雾/图标/黄圈渲染，以及不同第三方模组组合。归档本轮基础用例是非斩杀路径，不能据此宣称奖励界面验收通过；千足虫可重组节段沿用官方斩杀条件，不强制算作斩杀。

本轮未导出PCK、未部署到游戏。最终工作区DLL为Beta构建；进入游戏前需按项目原有流程重新打包部署，才能同时更新逻辑与本地化。

## 日志与复跑

- [双版本复跑脚本](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/run_branch.ps1)：分别传入 `Stable` 或 `Beta`；包含构建、私有测试副本准备和完整测试矩阵。
- [Stable构建日志](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/build-Stable.log)；[Beta构建日志](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/build-Beta.log)。
- [Stable中文基础快照](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/results-Stable-zhs-d0.json)；[Beta中文基础快照](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/results-Beta-zhs-d0.json)。其他语言/难度结果在同目录。
- [Stable专项日志](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/run-Stable-focused.log)；[Beta专项日志](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/run-Beta-focused.log)。
- [相对修改前备份的源码差异](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/audit-diff.txt)；[本地化键差异与缺失检查](C:/Users/Chirpchump/.codex/visualizations/2026/10/08/01a11b20-415c-79a2-bc58-c0b7d12cd428/full-card-audit-20261030/change-summary.json)。
