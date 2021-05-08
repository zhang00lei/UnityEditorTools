---@class DailyRewardConfig
---@field id number @id对应奖励id
---@field numVal number @number值
---@field booleanVal boolean @boolean值
---@field stringVal string @string值
---@field tableVal table @table值
local DailyRewardConfig = {
    [1] = {
        id = 1,
        numVal = 1,
        booleanVal = True,
        stringVal = "helloworld",
        tableVal = {id=1,name="zhangsan"},
    },
    [2] = {
        id = 2,
        numVal = 2,
        booleanVal = True,
        stringVal = "helloworld",
        tableVal = {id=2,name="zhangsan"},
    },
    [3] = {
        id = 3,
        numVal = 3,
        booleanVal = False,
        stringVal = "helloworld",
        tableVal = {id=3,name="zhangsan"},
    }
}
return DailyRewardConfig