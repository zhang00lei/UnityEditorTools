---this file is generate by tools,do not modify it.
---@class UISettingElements
local UISettingElements = {}
---Init 初始化UI元素
---@param tableInfo UISetting
function UISettingElements:Init(tableInfo)
    ---@type UnityEngine.GameObject
    tableInfo.Text = tableInfo:AddComponent(UnityEngine.GameObject, "Text")
    ---@type UnityEngine.GameObject
    tableInfo.TestText = tableInfo:AddComponent(UnityEngine.GameObject, "TestText")
    ---@type UIImage
    tableInfo.Image = tableInfo:AddComponent(UIImage, "Image")
end

function UISettingElements:Destroy(tableInfo)
    tableInfo.Text = nil
    tableInfo.TestText = nil
    tableInfo.Image = nil
end

return UISettingElements
