using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteDB;

namespace NSS_3330S
{
    public class DBExChipPkInfo : SQLiteBase
    {
        public string strTableName = "PROC_INFO_CHIP_PK";

        public DBExChipPkInfo(int nType)
        {
            TABLE_NAME = string.Format("{0}_{1}", strTableName, nType);

            // 테이블 생성 쿼리
            strQueryCreate = @"CREATE TABLE {0}(
                                Time BIGINT NOT NULL DEFAULT 0,
                                InTime VARCHAR(5000) NOT NULL DEFAULT '',
                                OutTime VARCHAR(5000) NOT NULL DEFAULT '',
                                LotId VARCHAR(50) NOT NULL DEFAULT '',
                                MgzId VARCHAR(50) NOT NULL DEFAULT '',
                                StripId VARCHAR(100) NOT NULL DEFAULT '', 
                                MagazineSlotNo INT NOT NULL DEFAULT 0,                                
                                CuttingTableNo INT NOT NULL DEFAULT 0,
                                MapStageNo INT NOT NULL DEFAULT 0,
                                RcpMat VARCHAR(100) NOT NULL DEFAULT '', 
                                HostCode VARCHAR(500) NOT NULL DEFAULT '',
                                TopCoodi VARCHAR(5000) NOT NULL DEFAULT '',
                                BtmCoodi VARCHAR(5000) NOT NULL DEFAULT '',
                                TopNgCode VARCHAR(500) NOT NULL DEFAULT '',
                                BtmNgCode VARCHAR(500) NOT NULL DEFAULT '',
                                TopInspResult VARCHAR(500) NOT NULL DEFAULT '',
                                BtmInspResult VARCHAR(500) NOT NULL DEFAULT '', 
                                TopPkgSize VARCHAR(5000) NOT NULL DEFAULT '',
                                BtmPkgSize VARCHAR(5000) NOT NULL DEFAULT '', 
                                BtmSawOffset VARCHAR(5000) NOT NULL DEFAULT '',
                                OutPort VARCHAR(500) NOT NULL DEFAULT '',
                                IsUnit VARCHAR(500) NOT NULL DEFAULT '')";

            strQueryCreate = string.Format(strQueryCreate, TABLE_NAME);

            DBPath = PathMgr.Inst.DBPath;

            CreateDbAndTable();
        }

    }
}
