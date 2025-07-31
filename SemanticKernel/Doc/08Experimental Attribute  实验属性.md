# Experimental Features in Semantic Kernel Semantic Kernel中的实验特征

- 03/06/2025




Semantic Kernel introduces experimental features to provide early access to new, evolving capabilities. These features allow users to explore cutting-edge functionality, but they are not yet stable and may be modified, deprecated, or removed in future releases.
Semantic Kernel引入了实验性功能，以提供对不断发展的新功能的早期访问。这些功能允许用户探索尖端功能，但它们尚不稳定，可能会在未来的版本中被修改、弃用或删除。



## Purpose of Experimental Features 实验功能的目的

The `Experimental` attribute serves several key purposes:
`实验性`属性有几个关键用途：

- **Signals Instability** – Indicates that a feature is still evolving and not yet production-ready.
  **信号不稳定** – 表示功能仍在发展中，尚未投入生产。
- **Encourages Early Feedback** – Allows developers to test and provide input before a feature is fully stabilized.
  **鼓励早期反馈** – 允许开发人员在功能完全稳定之前进行测试并提供输入。
- **Manages Expectations** – Ensures users understand that experimental features may have limited support or documentation.
  **管理期望** – 确保用户了解实验性功能可能具有有限的支持或文档。
- **Facilitates Rapid Iteration** – Enables the team to refine and improve features based on real-world usage.
  **促进快速迭代** – 使团队能够根据实际使用情况完善和改进功能。
- **Guides Contributors** – Helps maintainers and contributors recognize that the feature is subject to significant changes.
  **指导贡献者** – 帮助维护者和贡献者认识到该功能可能会发生重大更改。



## Implications for Users  对用户的影响

Using experimental features comes with certain considerations:
使用实验性功能需要注意以下几点：

- **Potential Breaking Changes** – APIs, behavior, or entire features may change without prior notice.
  **潜在的重大更改** – API、行为或整个功能可能会更改，恕不另行通知。
- **Limited Support** – The Semantic Kernel team may provide limited or no support for experimental features.
  **有限支持** – Semantic Kernel团队可能会为实验性功能提供有限的支持或不提供支持。
- **Stability Concerns** – Features may be less stable and prone to unexpected behavior or performance issues.
  **稳定性问题** – 功能可能不太稳定，并且容易出现意外行为或性能问题。
- **Incomplete Documentation** – Experimental features may have incomplete or outdated documentation.
  文档**不完整** – 实验性功能可能有不完整或过时的文档。



### Suppressing Experimental Feature Warnings in .NET 禁止显示 .NET 中的实验性功能警告

In the .NET SDK, experimental features generate compiler warnings. To suppress these warnings in your project, add the relevant diagnostic IDs to your `.csproj` file:
在 .NET SDK 中，实验性功能会生成编译器警告。若要在项目中禁止显示这些警告，请将相关诊断 ID 添加到 `.csproj` 文件：

XMLCopy  复制

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);SKEXP0001,SKEXP0010</NoWarn>
</PropertyGroup>
```

Each experimental feature has a unique diagnostic code (`SKEXPXXXX`). The full list can be found in **EXPERIMENTS.md**.
每个实验特征都有一个唯一的诊断代码 （`SKEXPXXXX）。` 完整列表可以在 **EXPERIMENTS.md** 中找到。



## Using Experimental Features in .NET 在 .NET 中使用实验性功能

In .NET, experimental features are marked using the `[Experimental]` attribute:
在 .NET 中，实验性功能使用 `[Experimental]` 属性进行标记：

C#Copy  复制

```csharp
using System;
using System.Diagnostics.CodeAnalysis;

[Experimental("SKEXP0101", "FeatureCategory")]
public class NewFeature 
{
    public void ExperimentalMethod() 
    {
        Console.WriteLine("This is an experimental feature.");
    }
}
```



### Experimental Feature Support in Other SDKs 其他 SDK 中的实验性功能支持

- **Python and Java** do not have a built-in experimental feature system like .NET.
  **Python 和 Java** 没有像 .NET 那样内置的实验性特征系统。
- Experimental features in **Python** may be marked using warnings (e.g., `warnings.warn`).
  **Python** 中的实验性功能可以使用警告（例如 `warnings.warn`）进行标记。
- In **Java**, developers typically use custom annotations to indicate experimental features.
  在 **Java** 中，开发人员通常使用自定义注释来指示实验性功能。



## Developing and Contributing to Experimental Features 开发和贡献实验性功能



### Marking a Feature as Experimental 将功能标记为实验性

- Apply the `Experimental` attribute to classes, methods, or properties:
  将 `Experimental` 属性应用于类、方法或属性：

C#Copy  复制

```csharp
[Experimental("SKEXP0101", "FeatureCategory")]
public class NewFeature { }
```

- Include a brief description explaining why the feature is experimental.
  包括一个简短的描述，解释为什么该功能是实验性的。
- Use meaningful tags as the second argument to categorize and track experimental features.
  使用有意义的标记作为第二个参数来对实验特征进行分类和跟踪。



### Coding and Documentation Best Practices 编码和文档最佳实践

- **Follow Coding Standards** – Maintain Semantic Kernel's coding conventions.
  **遵循编码标准** – 维护Semantic Kernel的编码约定。
- **Write Unit Tests** – Ensure basic functionality and prevent regressions.
  **编写单元测试** – 确保基本功能并防止回归。
- **Document All Changes** – Update relevant documentation, including `EXPERIMENTS.md`.
  **记录所有更改** – 更新相关文档，包括 `EXPERIMENTS.md`。
- **Use GitHub for Discussions** – Open issues or discussions to gather feedback.
  **使用 GitHub 进行讨论** – 打开问题或讨论以收集反馈。
- **Consider Feature Flags** – Where appropriate, use feature flags to allow opt-in/opt-out.
  **考虑功能标志** – 在适当的情况下，使用功能标志来允许选择加入/选择退出。



### Communicating Changes  沟通更改

- Clearly document updates, fixes, or breaking changes.
  清楚地记录更新、修复或重大更改。
- Provide migration guidance if the feature is evolving.
  如果功能正在发展，请提供迁移指导。
- Tag the relevant GitHub issues for tracking progress.
  标记相关的 GitHub 问题以跟踪进度。



## Future of Experimental Features 实验功能的未来

Experimental features follow one of three paths:
实验性功能遵循以下三种路径之一：

1. **Graduation to Stable** – If a feature is well-received and technically sound, it may be promoted to stable.
   升级**到稳定** – 如果某个功能受到好评且技术合理，则可能会将其提升为稳定版。
2. **Deprecation & Removal** – Features that do not align with long-term goals may be removed.
   **弃用和删除** – 与长期目标不一致的功能可能会被删除。
3. **Continuous Experimentation** – Some features may remain experimental indefinitely while being iterated upon.
   **持续试验** – 某些功能在迭代时可能会无限期地保持实验状态。

The Semantic Kernel team strives to communicate experimental feature updates through release notes and documentation updates.
Semantic Kernel团队致力于通过发行说明和文档更新来传达实验性功能更新。



## Getting Involved  参与其中

The community plays a crucial role in shaping the future of experimental features. Provide feedback via:
社区在塑造实验功能的未来方面发挥着至关重要的作用。通过以下方式提供反馈：

- **GitHub Issues** – Report bugs, request improvements, or share concerns.
  **GitHub 问题** – 报告错误、请求改进或分享疑虑。
- **Discussions & PRs** – Engage in discussions and contribute directly to the codebase.
  **讨论和 PR** – 参与讨论并直接为代码库做出贡献。



## Summary  总结

- **Experimental features** allow users to test and provide feedback on new capabilities in Semantic Kernel.
  **实验性功能**允许用户测试Semantic Kernel中的新功能并提供反馈。
- **They may change frequently**, have limited support, and require caution when used in production.
  **它们可能经常更换** ，支持有限，并且在生产中使用时需要谨慎。
- **Contributors should follow best practices**, use `[Experimental]` correctly, and document changes properly.
  **贡献者应遵循最佳实践** ，正确使用 `[实验性]`，并正确记录更改。
- **Users can suppress warnings** for experimental features but should stay updated on their evolution.
  **用户可以抑制**实验性功能的警告，但应随时了解其演变。

For the latest details, check **EXPERIMENTS.md**.
有关最新详细信息，请查看 **EXPERIMENTS.md**。