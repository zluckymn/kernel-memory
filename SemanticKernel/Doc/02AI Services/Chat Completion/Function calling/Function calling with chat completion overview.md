# Function calling with chat completion 使用聊天完成进行函数调用

- 04/16/2025

The most powerful feature of chat completion is the ability to call functions from the model. This allows you to create a chat bot that can interact with your existing code, making it possible to automate business processes, create code snippets, and more.
聊天完成最强大的功能是能够从模型调用函数。这允许您创建一个可以与现有代码交互的聊天机器人，从而可以自动化业务流程、创建代码片段等。

With Semantic Kernel, we simplify the process of using function calling by automatically describing your functions and their parameters to the model and then handling the back-and-forth communication between the model and your code.
借助Kernel，我们通过自动向模型描述函数及其参数，然后处理模型与代码之间的来回通信，简化了使用函数调用的过程。

When using function calling, however, it's good to understand what's *actually* happening behind the scenes so that you can optimize your code and make the most of this feature.
但是，在使用函数调用时，最好了解幕后*实际*发生的情况，以便优化代码并充分利用此功能。



## How auto function calling works 自动函数调用的工作原理

 Note  注意

The following section describes how auto function calling works in Semantic Kernel. Auto function calling is the default behavior in Semantic Kernel, but you can also manually invoke functions if you prefer. For more information on manual function invocation, please refer to the [function invocation article](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-invocation#manual-function-invocation).
以下部分介绍自动函数调用在Kernel中的工作原理。自动函数调用是Kernel中的默认行为，但如果你愿意，也可以手动调用函数。有关手动函数调用的更多信息，请参阅[函数调用文章 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-invocation#manual-function-invocation)。

When you make a request to a model with function calling enabled, Semantic Kernel performs the following steps:
当您向启用了函数调用的模型发出请求时，Kernel会执行以下步骤：

  展开表

| #    | Step  步                                                     | Description  描述                                            |
| :--- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 1    | [**Serialize functions  序列化函数**](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp#1-serializing-the-functions) | All of the available functions (and its input parameters) in the kernel are serialized using JSON schema. 内核中的所有可用函数（及其输入参数）都使用 JSON 模式进行序列化。 |
| 2    | [**Send the messages and functions to the model将消息和函数发送到模型**](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp#2-sending-the-messages-and-functions-to-the-model) | The serialized functions (and the current chat history) are sent to the model as part of the input. 序列化函数（和当前聊天记录）作为输入的一部分发送到模型。 |
| 3    | [**Model processes the input模型处理输入**](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp#3-model-processes-the-input) | The model processes the input and generates a response. The response can either be a chat message or one or more function calls. 模型处理输入并生成响应。响应可以是聊天消息，也可以是一个或多个函数调用。 |
| 4    | [**Handle the response  处理响应**](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp#4-handle-the-response) | If the response is a chat message, it is returned to the caller. If the response is a function call, however, Semantic Kernel extracts the function name and its parameters. 如果响应是聊天消息，则返回给调用方。但是，如果响应是函数调用，则Kernel会提取函数名称及其参数。 |
| 5    | [**Invoke the function  调用函数**](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp#5-invoke-the-function) | The extracted function name and parameters are used to invoke the function in the kernel. 提取的函数名称和参数用于在内核中调用函数。 |
| 6    | [**Return the function result返回函数结果**](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp#6-return-the-function-result) | The result of the function is then sent back to the model as part of the chat history. Steps 2-6 are then repeated until the model returns a chat message or the max iteration number has been reached. 然后，该函数的结果将作为聊天记录的一部分发送回模型。然后重复步骤 2-6，直到模型返回聊天消息或达到最大迭代次数。 |

The following diagram illustrates the process of function calling:
下图演示了函数调用的过程：

![Semantic Kernel function calling](https://learn.microsoft.com/en-us/semantic-kernel/media/functioncalling.png)

The following section will use a concrete example to illustrate how function calling works in practice.
以下部分将使用一个具体示例来说明函数调用在实践中的工作原理。



## Example: Ordering a pizza 示例：订购披萨

Let's assume you have a plugin that allows a user to order a pizza. The plugin has the following functions:
假设您有一个允许用户订购披萨的插件。该插件具有以下功能：

1. `get_pizza_menu`: Returns a list of available pizzas
   `get_pizza_menu`：返回可用比萨饼的列表
2. `add_pizza_to_cart`: Adds a pizza to the user's cart
   `add_pizza_to_cart`：将披萨添加到用户的购物车
3. `remove_pizza_from_cart`: Removes a pizza from the user's cart
   `remove_pizza_from_cart`：从用户的购物车中删除披萨
4. `get_pizza_from_cart`: Returns the specific details of a pizza in the user's cart
   `get_pizza_from_cart`：返回用户购物车中披萨的具体详细信息
5. `get_cart`: Returns the user's current cart
   `get_cart`：返回用户的当前购物车
6. `checkout`: Checks out the user's cart
   `checkout`：签出用户的购物车

In C#, the plugin might look like this:
在 C# 中，插件可能如下所示：

C#Copy  复制

```csharp
public class OrderPizzaPlugin(
    IPizzaService pizzaService,
    IUserContext userContext,
    IPaymentService paymentService)
{
    [KernelFunction("get_pizza_menu")]
    public async Task<Menu> GetPizzaMenuAsync()
    {
        return await pizzaService.GetMenu();
    }

    [KernelFunction("add_pizza_to_cart")]
    [Description("Add a pizza to the user's cart; returns the new item and updated cart")]
    public async Task<CartDelta> AddPizzaToCart(
        PizzaSize size,
        List<PizzaToppings> toppings,
        int quantity = 1,
        string specialInstructions = ""
    )
    {
        Guid cartId = userContext.GetCartId();
        return await pizzaService.AddPizzaToCart(
            cartId: cartId,
            size: size,
            toppings: toppings,
            quantity: quantity,
            specialInstructions: specialInstructions);
    }

    [KernelFunction("remove_pizza_from_cart")]
    public async Task<RemovePizzaResponse> RemovePizzaFromCart(int pizzaId)
    {
        Guid cartId = userContext.GetCartId();
        return await pizzaService.RemovePizzaFromCart(cartId, pizzaId);
    }

    [KernelFunction("get_pizza_from_cart")]
    [Description("Returns the specific details of a pizza in the user's cart; use this instead of relying on previous messages since the cart may have changed since then.")]
    public async Task<Pizza> GetPizzaFromCart(int pizzaId)
    {
        Guid cartId = await userContext.GetCartIdAsync();
        return await pizzaService.GetPizzaFromCart(cartId, pizzaId);
    }

    [KernelFunction("get_cart")]
    [Description("Returns the user's current cart, including the total price and items in the cart.")]
    public async Task<Cart> GetCart()
    {
        Guid cartId = await userContext.GetCartIdAsync();
        return await pizzaService.GetCart(cartId);
    }

    [KernelFunction("checkout")]
    [Description("Checkouts the user's cart; this function will retrieve the payment from the user and complete the order.")]
    public async Task<CheckoutResponse> Checkout()
    {
        Guid cartId = await userContext.GetCartIdAsync();
        Guid paymentId = await paymentService.RequestPaymentFromUserAsync(cartId);

        return await pizzaService.Checkout(cartId, paymentId);
    }
}
```

You would then add this plugin to the kernel like so:
然后，您将此插件添加到内核中，如下所示：

C#Copy  复制

```csharp
IKernelBuilder kernelBuilder = new KernelBuilder();
kernelBuilder..AddAzureOpenAIChatCompletion(
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT"
);
kernelBuilder.Plugins.AddFromType<OrderPizzaPlugin>("OrderPizza");
Kernel kernel = kernelBuilder.Build();
```

 Note  注意

Only functions with the `KernelFunction` attribute will be serialized and sent to the model. This allows you to have helper functions that are not exposed to the model.
只有具有 `KernelFunction` 属性的函数才会被序列化并发送到模型。这允许您拥有未向模型公开的辅助函数。



### 1) Serializing the functions 1） 序列化函数

When you create a kernel with the `OrderPizzaPlugin`, the kernel will automatically serialize the functions and their parameters. This is necessary so that the model can understand the functions and their inputs.
当您使用 `OrderPizzaPlugin` 创建内核时，内核将自动序列化函数及其参数。这是必要的，以便模型能够理解函数及其输入。

For the above plugin, the serialized functions would look like this:
对于上面的插件，序列化函数将如下所示：

JSONCopy  复制

```json
[
  {
    "type": "function",
    "function": {
      "name": "OrderPizza-get_pizza_menu",
      "parameters": {
        "type": "object",
        "properties": {},
        "required": []
      }
    }
  },
  {
    "type": "function",
    "function": {
      "name": "OrderPizza-add_pizza_to_cart",
      "description": "Add a pizza to the user's cart; returns the new item and updated cart",
      "parameters": {
        "type": "object",
        "properties": {
          "size": {
            "type": "string",
            "enum": ["Small", "Medium", "Large"]
          },
          "toppings": {
            "type": "array",
            "items": {
              "type": "string",
              "enum": ["Cheese", "Pepperoni", "Mushrooms"]
            }
          },
          "quantity": {
            "type": "integer",
            "default": 1,
            "description": "Quantity of pizzas"
          },
          "specialInstructions": {
            "type": "string",
            "default": "",
            "description": "Special instructions for the pizza"
          }
        },
        "required": ["size", "toppings"]
      }
    }
  },
  {
    "type": "function",
    "function": {
      "name": "OrderPizza-remove_pizza_from_cart",
      "parameters": {
        "type": "object",
        "properties": {
          "pizzaId": {
            "type": "integer"
          }
        },
        "required": ["pizzaId"]
      }
    }
  },
  {
    "type": "function",
    "function": {
      "name": "OrderPizza-get_pizza_from_cart",
      "description": "Returns the specific details of a pizza in the user's cart; use this instead of relying on previous messages since the cart may have changed since then.",
      "parameters": {
        "type": "object",
        "properties": {
          "pizzaId": {
            "type": "integer"
          }
        },
        "required": ["pizzaId"]
      }
    }
  },
  {
    "type": "function",
    "function": {
      "name": "OrderPizza-get_cart",
      "description": "Returns the user's current cart, including the total price and items in the cart.",
      "parameters": {
        "type": "object",
        "properties": {},
        "required": []
      }
    }
  },
  {
    "type": "function",
    "function": {
      "name": "OrderPizza-checkout",
      "description": "Checkouts the user's cart; this function will retrieve the payment from the user and complete the order.",
      "parameters": {
        "type": "object",
        "properties": {},
        "required": []
      }
    }
  }
]
```

There's a few things to note here which can impact both the performance and the quality of the chat completion:
这里需要注意一些事项，这些事项可能会影响聊天完成的性能和质量：

1. **Verbosity of function schema** – Serializing functions for the model to use doesn't come for free. The more verbose the schema, the more tokens the model has to process, which can slow down the response time and increase costs.
   **函数架构的详细程度** – 序列化模型使用的函数不是免费的。架构越冗长，模型必须处理的令牌就越多，这会减慢响应时间并增加成本。

    Tip  提示

   Keep your functions as simple as possible. In the above example, you'll notice that not *all* functions have descriptions where the function name is self-explanatory. This is intentional to reduce the number of tokens. The parameters are also kept simple; anything the model shouldn't need to know (like the `cartId` or `paymentId`) are kept hidden. This information is instead provided by internal services.
   让您的功能尽可能简单。在上面的示例中，您会注意到并非*所有*函数都有函数名称不言自明的描述。这是为了减少令牌的数量。参数也保持简单;模型不需要知道的任何内容（如 `cartId` 或 `paymentId`）都会被隐藏。相反，此信息由内部服务提供。

    Note  注意

   The one thing you don't need to worry about is the complexity of the return types. You'll notice that the return types are not serialized in the schema. This is because the model doesn't need to know the return type to generate a response. In Step 6, however, we'll see how overly verbose return types can impact the quality of the chat completion.
   您无需担心的一件事是返回类型的复杂性。您会注意到，返回类型未在架构中序列化。这是因为模型不需要知道返回类型即可生成响应。但是，在步骤 6 中，我们将了解过于冗长的返回类型如何影响聊天完成的质量。

2. **Parameter types** – With the schema, you can specify the type of each parameter. This is important for the model to understand the expected input. In the above example, the `size` parameter is an enum, and the `toppings` parameter is an array of enums. This helps the model generate more accurate responses.
   **参数类型** — 使用架构，您可以指定每个参数的类型。这对于模型理解预期输入非常重要。在上面的示例中，`size` 参数是枚举，`toppings` 参数是枚举数组。这有助于模型生成更准确的响应。

    Tip  提示

   Avoid, where possible, using `string` as a parameter type. The model can't infer the type of string, which can lead to ambiguous responses. Instead, use enums or other types (e.g., `int`, `float`, and complex types) where possible.
   尽可能避免使用 `string` 作为参数类型。模型无法推断字符串的类型，这可能导致响应不明确。相反，尽可能使用枚举或其他类型（例如 `int`、`float` 和复杂类型）。

3. **Required parameters** - You can also specify which parameters are required. This is important for the model to understand which parameters are *actually* necessary for the function to work. Later on in Step 3, the model will use this information to provide as minimal information as necessary to call the function.
   **必需参数** - 您还可以指定哪些参数是必需的。这对于模型了解函数工作*实际上需要哪些*参数非常重要。稍后在步骤 3 中，模型将使用此信息提供调用函数所需的最少信息。

    Tip  提示

   Only mark parameters as required if they are *actually* required. This helps the model call functions more quickly and accurately.
   仅当参数*实际*需要时，才将其标记为必需参数。这有助于模型更快、更准确地调用函数。

4. **Function descriptions** – Function descriptions are optional but can help the model generate more accurate responses. In particular, descriptions can tell the model what to expect from the response since the return type is not serialized in the schema. If the model is using functions improperly, you can also add descriptions to provide examples and guidance.
   **函数描述** – 函数描述是可选的，但可以帮助模型生成更准确的响应。特别是，描述可以告诉模型从响应中得到什么，因为返回类型未在架构中序列化。如果模型使用函数不正确，您还可以添加描述以提供示例和指导。

   For example, in the `get_pizza_from_cart` function, the description tells the user to use this function instead of relying on previous messages. This is important because the cart may have changed since the last message.
   例如，在 `get_pizza_from_cart` 函数中，描述告诉用户使用此函数，而不是依赖以前的消息。这很重要，因为购物车可能自上一条消息以来已更改。

    Tip  提示

   Before adding a description, ask yourself if the model *needs* this information to generate a response. If not, consider leaving it out to reduce verbosity. You can always add descriptions later if the model is struggling to use the function properly.
   在添加描述之前，请问问自己模型是否*需要*此信息来生成响应。如果没有，请考虑将其省略以减少冗长。如果模型难以正确使用该函数，您以后始终可以添加描述。

5. **Plugin name** – As you can see in the serialized functions, each function has a `name` property. Semantic Kernel uses the plugin name to namespace the functions. This is important because it allows you to have multiple plugins with functions of the same name. For example, you may have plugins for multiple search services, each with their own `search` function. By namespacing the functions, you can avoid conflicts and make it easier for the model to understand which function to call.
   **插件名称** – 正如您在序列化函数中看到的那样，每个函数都有一个 `name` 属性。Kernel使用插件名称为函数命名空间。这很重要，因为它允许您拥有多个具有相同名称功能的插件。例如，您可能有多个搜索服务的插件，每个服务都有自己的`搜索`功能。通过对函数进行命名空间，可以避免冲突，并使模型更容易理解要调用的函数。

   Knowing this, you should choose a plugin name that is unique and descriptive. In the above example, the plugin name is `OrderPizza`. This makes it clear that the functions are related to ordering pizza.
   知道这一点后，您应该选择一个独特且具有描述性的插件名称。在上面的示例中，插件名称为 `OrderPizza`。这清楚地表明这些功能与订购披萨有关。

    Tip  提示

   When choosing a plugin name, we recommend removing superfluous words like "plugin" or "service". This helps reduce verbosity and makes the plugin name easier to understand for the model.
   选择插件名称时，我们建议删除多余的词，例如“插件”或“服务”。这有助于减少冗长性，并使模型更容易理解插件名称。

    Note  注意

   By default, the delimiter for the function name is `-`. While this works for most models, some of them may have different requirements, such as [Gemini](https://ai.google.dev/gemini-api/docs/function-calling#key-parameters-best-practices). This is taken care of by the kernel automatically however you may see slightly different function names in the serialized functions.
   默认情况下，函数名称的分隔符为 `-`。虽然这适用于大多数型号，但其中一些可能有不同的要求，例如 [Gemini](https://ai.google.dev/gemini-api/docs/function-calling#key-parameters-best-practices)。这由内核自动处理，但是您可能会在序列化函数中看到略有不同的函数名称。



### 2) Sending the messages and functions to the model 2）向模型发送消息和函数

Once the functions are serialized, they are sent to the model along with the current chat history. This allows the model to understand the context of the conversation and the available functions.
函数序列化后，它们将与当前聊天记录一起发送到模型。这使得模型能够理解对话的上下文和可用功能。

In this scenario, we can imagine the user asking the assistant to add a pizza to their cart:
在这种情况下，我们可以想象用户要求助手将披萨添加到购物车中：

C#Copy  复制

```csharp
ChatHistory chatHistory = [];
chatHistory.AddUserMessage("I'd like to order a pizza!");
```

We can then send this chat history and the serialized functions to the model. The model will use this information to determine the best way to respond.
然后我们可以将此聊天记录和序列化函数发送到模型。模型将使用此信息来确定最佳响应方式。

C#Copy  复制

```csharp
IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

ChatResponse response = await chatCompletion.GetChatMessageContentAsync(
    chatHistory,
    executionSettings: openAIPromptExecutionSettings,
    kernel: kernel)
```

 Note  注意

This example uses the `FunctionChoiceBehavior.Auto()` behavior, one of the few available ones. For more information about other function choice behaviors, check out the [function choice behaviors article](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors).
此示例使用 `FunctionChoiceBehavior.Auto（）` 行为，这是为数不多的可用行为之一。有关其他函数选择行为的更多信息，请查看[函数选择行为一文 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors)。

 Important  重要

The kernel must be passed to the service in order to use function calling. This is because the plugins are registered with the kernel, and the service needs to know which plugins are available.
必须将内核传递给服务才能使用函数调用。这是因为插件已向内核注册，并且服务需要知道哪些插件可用。



### 3) Model processes the input 3）模型处理输入

With both the chat history and the serialized functions, the model can determine the best way to respond. In this case, the model recognizes that the user wants to order a pizza. The model would likely *want* to call the `add_pizza_to_cart` function, but because we specified the size and toppings as required parameters, the model will ask the user for this information:
通过聊天历史记录和序列化函数，模型可以确定最佳响应方式。在这种情况下，模型识别用户想要订购披萨。模型可能*想要*调用 `add_pizza_to_cart` 函数，但由于我们将 size 和 toppings 指定为必需参数，因此模型将要求用户提供以下信息：

C#Copy  复制

```csharp
Console.WriteLine(response);
chatHistory.AddAssistantMessage(response);

// "Before I can add a pizza to your cart, I need to
// know the size and toppings. What size pizza would
// you like? Small, medium, or large?"
```

Since the model wants the user to respond next, Semantic Kernel will stop automatic function calling and return control to the user. At this point, the user can respond with the size and toppings of the pizza they want to order:
由于模型希望用户接下来做出响应，因此Kernel将停止自动函数调用并将控制权返回给用户。此时，用户可以回复他们想要订购的披萨的大小和配料：

C#Copy  复制

```csharp
chatHistory.AddUserMessage("I'd like a medium pizza with cheese and pepperoni, please.");

response = await chatCompletion.GetChatMessageContentAsync(
    chatHistory,
    kernel: kernel)
```

Now that the model has the necessary information, it can now call the `add_pizza_to_cart` function with the user's input. Behind the scenes, it adds a new message to the chat history that looks like this:
现在模型拥有必要的信息，它现在可以使用用户的输入调用 `add_pizza_to_cart` 函数。在幕后，它向聊天记录添加了一条新消息，如下所示：

C#Copy  复制

```csharp
"tool_calls": [
    {
        "id": "call_abc123",
        "type": "function",
        "function": {
            "name": "OrderPizzaPlugin-add_pizza_to_cart",
            "arguments": "{\n\"size\": \"Medium\",\n\"toppings\": [\"Cheese\", \"Pepperoni\"]\n}"
        }
    }
]
```

 Tip  提示

It's good to remember that every argument you require must be generated by the model. This means spending tokens to generate the response. Avoid arguments that require many tokens (like a GUID). For example, notice that we use an `int` for the `pizzaId`. Asking the model to send a one to two digit number is much easier than asking for a GUID.
最好记住，您需要的每个参数都必须由模型生成。这意味着花费代币来生成响应。避免使用需要许多标记的参数 （，如 GUID） 。例如，请注意，我们对 `pizzaId` 使用了一个 `int`。要求模型发送一到两位数字比请求 GUID 容易得多。

 Important  重要

This step is what makes function calling so powerful. Previously, AI app developers had to create separate processes to extract intent and slot fill functions. With function calling, the model can decide *when* to call a function and *what* information to provide.
这一步使函数调用如此强大。以前，AI 应用程序开发人员必须创建单独的流程来提取意图和插槽填充函数。通过函数调用，模型可以决定*何时*调用函数以及提供*哪些*信息。



### 4) Handle the response  4）处理响应

When Semantic Kernel receives the response from the model, it checks if the response is a function call. If it is, Semantic Kernel extracts the function name and its parameters. In this case, the function name is `OrderPizzaPlugin-add_pizza_to_cart`, and the arguments are the size and toppings of the pizza.
当Kernel收到来自模型的响应时，它会检查响应是否是函数调用。如果是，Kernel将提取函数名称及其参数。在本例中，函数名称为 `OrderPizzaPlugin-add_pizza_to_cart` ，参数是披萨的大小和配料。

With this information, Semantic Kernel can marshal the inputs into the appropriate types and pass them to the `add_pizza_to_cart` function in the `OrderPizzaPlugin`. In this example, the arguments originate as a JSON string but are deserialized by Semantic Kernel into a `PizzaSize` enum and a `List<PizzaToppings>`.
有了这些信息，Kernel可以将输入封送到适当的类型中，并将它们传递给 `OrderPizzaPlugin` 中的 `add_pizza_to_cart` 函数。在此示例中，参数源自 JSON 字符串，但由Kernel反序列化为 `PizzaSize` 枚举和 `List<PizzaToppings>`。

 Note  注意

Marshaling the inputs into the correct types is one of the key benefits of using Semantic Kernel. Everything from the model comes in as a JSON object, but Semantic Kernel can automatically deserialize these objects into the correct types for your functions.
将输入封送到正确的类型中是使用Kernel的主要好处之一。模型中的所有内容都以 JSON 对象的形式出现，但Kernel可以自动将这些对象反序列化为函数的正确类型。

After marshalling the inputs, Semantic Kernel will also add the function call to the chat history:
封送输入后，Kernel还会将函数调用添加到聊天记录中：

C#Copy  复制

```csharp
chatHistory.Add(
    new() {
        Role = AuthorRole.Assistant,
        Items = [
            new FunctionCallContent(
                functionName: "add_pizza_to_cart",
                pluginName: "OrderPizza",
                id: "call_abc123",
                arguments: new () { {"size", "Medium"}, {"toppings", ["Cheese", "Pepperoni"]} }
            )
        ]
    }
);
```



### 5) Invoke the function  5）调用函数

Once Semantic Kernel has the correct types, it can finally invoke the `add_pizza_to_cart` function. Because the plugin uses dependency injection, the function can interact with external services like `pizzaService` and `userContext` to add the pizza to the user's cart.
一旦Kernel具有正确的类型，它最终就可以调用 `add_pizza_to_cart` 函数。由于该插件使用依赖注入，因此该函数可以与 `pizzaService` 和 `userContext` 等外部服务交互，以将披萨添加到用户的购物车中。

Not all functions will succeed, however. If the function fails, Semantic Kernel can handle the error and provide a default response to the model. This allows the model to understand what went wrong and decide to retry or generate a response to the user.
然而，并非所有功能都会成功。如果函数失败，Kernel可以处理错误并向模型提供默认响应。这使模型能够了解出了什么问题，并决定重试或生成对用户的响应。

 Tip  提示

To ensure a model can self-correct, it's important to provide error messages that clearly communicate what went wrong and how to fix it. This can help the model retry the function call with the correct information.
为确保模型能够自我纠正，提供错误消息以清楚地传达问题所在以及如何修复它非常重要。这可以帮助模型使用正确的信息重试函数调用。

 Note  注意

Semantic Kernel automatically invokes functions by default. However, if you prefer to manage function invocation manually, you can enable manual function invocation mode. For more details on how to do this, please refer to the [function invocation article](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-invocation).
默认情况下，Kernel会自动调用函数。但是，如果您希望手动管理函数调用，则可以启用手动函数调用模式。有关如何执行此作的更多详细信息，请参阅[函数调用文章 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-invocation)。



### 6) Return the function result 6）返回函数结果

After the function has been invoked, the function result is sent back to the model as part of the chat history. This allows the model to understand the context of the conversation and generate a subsequent response.
调用函数后，函数结果将作为聊天历史记录的一部分发送回模型。这使得模型能够理解对话的上下文并生成后续响应。

Behind the scenes, Semantic Kernel adds a new message to the chat history from the tool role that looks like this:
在幕后，Kernel从工具角色向聊天记录添加了一条新消息，如下所示：

C#Copy  复制

```csharp
chatHistory.Add(
    new() {
        Role = AuthorRole.Tool,
        Items = [
            new FunctionResultContent(
                functionName: "add_pizza_to_cart",
                pluginName: "OrderPizza",
                id: "0001",
                result: "{ \"new_items\": [ { \"id\": 1, \"size\": \"Medium\", \"toppings\": [\"Cheese\",\"Pepperoni\"] } ] }"
            )
        ]
    }
);
```

Notice that the result is a JSON string that the model then needs to process. As before, the model will need to spend tokens consuming this information. This is why it's important to keep the return types as simple as possible. In this case, the return only includes the new items added to the cart, not the entire cart.
请注意，结果是模型需要处理的 JSON 字符串。和以前一样，模型将需要花费代币来消费这些信息。这就是为什么保持返回类型尽可能简单很重要的原因。在这种情况下，退货仅包括添加到购物车的新商品，而不是整个购物车。

 Tip  提示

Be as succinct as possible with your returns. Where possible, only return the information the model needs or summarize the information using another LLM prompt before returning it.
您的退货尽可能简洁。在可能的情况下，仅返回模型需要的信息，或者在返回之前使用另一个 LLM 提示汇总信息。



### Repeat steps 2-6  重复步骤 2-6

After the result is returned to the model, the process repeats. The model processes the latest chat history and generates a response. In this case, the model might ask the user if they want to add another pizza to their cart or if they want to check out.
将结果返回给模型后，该过程将重复。该模型处理最新的聊天记录并生成响应。在这种情况下，模型可能会询问用户是否要将另一个披萨添加到购物车中，或者是否要结账。



## Parallel function calls  并行函数调用

In the above example, we demonstrated how an LLM can call a single function. Often this can be slow if you need to call multiple functions in sequence. To speed up the process, several LLMs support parallel function calls. This allows the LLM to call multiple functions at once, speeding up the process.
在上面的示例中，我们演示了 LLM 如何调用单个函数。如果您需要按顺序调用多个函数，这通常会很慢。为了加快该过程，一些 LLM 支持并行函数调用。这允许 LLM 一次调用多个函数，从而加快该过程。

For example, if a user wants to order multiple pizzas, the LLM can call the `add_pizza_to_cart` function for each pizza at the same time. This can significantly reduce the number of round trips to the LLM and speed up the ordering process.
例如，如果用户想要订购多个披萨，LLM 可以同时为每个披萨调用 `add_pizza_to_cart` 函数。这可以显着减少 LLM 的往返次数并加快订购过程。



## Next steps  后续步骤

Now that you understand how function calling works, you can proceed to learn how to configure various aspects of function calling that better correspond to your specific scenarios by going to the next step:
了解函数调用的工作原理后，可以通过转到下一步继续了解如何配置函数调用的各个方面，以更好地对应您的特定方案：

[  函数选择行为](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors)