//
//  _ModelName_Query.h
//
//  Created by 曾贵平 on _time_.
//

#ifndef ___ModelName_Query_H__
#define ___ModelName_Query_H__

#include "BaseQuery.h"

class _ModelName_Query :
	public BaseQuery
{
private:
	_ModelName_Query(void);
	~_ModelName_Query(void);
public:
	static _ModelName_Query* GetQuery();
	void Insert(_ModelName_ *pEntity);//插入一行数据
    void InsertWithID(_ModelName_ *pEntity);//插入一行数据
	void Update(_ModelName_ *pEntity);//更新一行数据
	_ModelName_ *SelectByID(<#PIDcase[INTEGER,REAL,TEXT]:[int;float;const char*]#> ID);//根据ID查找一行记录
    std::list<_ModelName_*> SelectByWhere(const char* where);//根据条件查找数据
    std::list<_ModelName_*> SelectAll();//查找所有数据
	void DeleteByID(<#PIDcase[INTEGER,REAL,TEXT]:[int;float;const char*]#> ID);//删除一行记录
};
#endif


