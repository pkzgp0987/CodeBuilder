//
//游戏中最基础的宏，包含有数据库文件名称的宏
//可以在该宏中添加游戏中统一的宏定义
//
//

#ifndef __GAME_DEFINE_H__
#define __GAME_DEFINE_H__

#include <string>
using namespace std;

//文件
#define DB_FILE_NAME "GameDB.sqlite" //游戏系统数据库文件名
#define LOG_DB_FILE_NAME "LOG.sqlite" //玩家日志数据库文件名
#define Role_DB_FILE_NAME "RoleDB.db" //玩家信息数据库文件名

#endif
