# 升级到 GPU 集群

### 重要：这个计划需要大家共同决定，请认真看完说明，权衡利弊

## 为什么

当前，组内的服务器使用情况存在以下问题：
* GPU 抢占式使用，GPU 的使用情况混乱。
* 服务器间数据迁移困难，一份 A 服务器上的代码在 B 服务器上运行之前，还需拷贝数据集等数据，服务器间存在大量重复数据。
* 环境公用，如果某人安装/更新了系统环境，可能会影响他人的使用。

同时，还存在账户管理，权限管理复杂等问题。

## 优点和缺点

### 优点
* GPU 隔离使用，使用 GPU 前需先申请，申请后自动调度合适数量的 GPU 独占使用，到期后自动回收
* 数据在服务器间共享，使用者拥有全局公用的数据集文件夹，以及每个使用者独立的 Home 和 Data 文件夹
* 使用者的环境相互独立，并拥有自己环境的管理员权限，可以随意调整自己环境的配置
* 镜像式管理，环境出错时可以还原

### 缺点
* **升级前需要对数据进行大规模迁移，存在数据丢失的风险**
* **同时会有一段时间服务器不可用**
* 目前的 GPU 调度方式只能实现每个人独占 1 块或几块 GPU，而不能把 1 块 GPU 共享给几个人同时使用，可能导致 GPU 使用率不高
* 每个人的环境需要重新配置
* 由于是个人开发，不可避免存在 bug

## 具体升级方式

GPU 集群升级，需要在现有系统上搭建 Kubernetes 集群、GlusterFS 文件系统，以及部署 Web 服务。为了搭建新的分布式文件系统，需要对现有数据做完全迁移。同时原有数据已不必要，可以对服务器做完整清理并重装系统。

如果执行这个升级计划，那么整个升级过程分三步：
1. 统计需要保留的数据集，每人拷走自己要存下来的代码和模型
2. 清空硬盘，重建文件系统，把保留的数据集放在公用的数据集文件夹下
3. 部署集群及 Web 服务，为每人分配账号，自行创建环境及拷贝代码模型

## 技术细节和演示

请参考 [docs/README.md](./docs/README.md)
