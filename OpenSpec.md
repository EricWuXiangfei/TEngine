## OpenSpec 已在你的项目中配置好了，生成了以下内容：

## Skills（技能）：
- openspec-propose - 提出新功能/修改方案
- openspec-explore - 探索现有代码库
- openspec-apply-change - 执行实现任务
- openspec-archive-change - 归档已完成的变更


## Commands（命令）：
- /opsx:propose - 开始一个新的功能提案
- /opsx:explore - 探索代码库
- /opsx:apply - 执行实现
- /opsx:archive - 归档变更

典型工作流
### 1. 提出新功能

在对话框中输入：
```
/opsx:propose "在TEngine中添加新的背包系统"
```
AI 会自动创建：
- openspec/changes/add-背包系统/proposal.md - 为什么要做这个，改什么
- openspec/changes/add-背包系统/specs/ - 需求和场景
- openspec/changes/add-背包系统/design.md - 技术方案
- openspec/changes/add-背包系统/tasks.md - 实现清单

### 2. 执行实现
```
/opsx:apply
```

### 3. 归档完成的变更
```
/opsx:archive
```