# High Level Visual Scripting System
Offers game designers an intuitive way to express ideas and realize game prototypes without dependencing on programming. 

Based on Unity's GraphView technology
Simple and powerful C# node API to create new nodes and custom views.

```CSharp
using System;
using GraphProcessor;
using UnityEngine;

[Serializable, NodeMenuItem("Gameobject/Has Tag")]
public class CompareTagNode : HlvsDataNode
{
    public override string name => "Has Tag";

    [Input("Object")]
    public GameObject target;

    [Input("Tag")]
    public string tag;

    [Output("Has Tag")]
    public bool hasTag;

    public override ProcessingStatus Evaluate()
    {
        hasTag = target && target.CompareTag(tag);
        return ProcessingStatus.Finished;
    }
}
```

## Gallery


### Node connection menu
![](https://user-images.githubusercontent.com/6877923/89330117-12f67d80-d690-11ea-9b62-f878b86b8342.gif)

### Node creation menu
![](https://user-images.githubusercontent.com/6877923/58935811-893adf80-876e-11e9-9f69-69ce51a432b8.png)
