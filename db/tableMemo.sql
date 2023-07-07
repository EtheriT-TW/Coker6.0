use [EPZA]
go
IF not exists(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
           BEGIN  
            exec sp_addextendedproperty 'MS_Description', '資料庫更新日誌', 'user', 'dbo', 'table', '__EFMigrationsHistory'
           END  
ELSE
           BEGIN  
            exec sp_updateextendedproperty 'MS_Description', '資料庫更新日誌', 'user', 'dbo', 'table', '__EFMigrationsHistory'
           END

go
IF not exists(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
           BEGIN  
            exec sp_addextendedproperty 'MS_Description', '文章', 'user', 'dbo', 'table', 'Article'
           END  
ELSE
           BEGIN  
            exec sp_updateextendedproperty 'MS_Description', '文章', 'user', 'dbo', 'table', 'Article'
           END

go
IF not exists(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
           BEGIN  
            exec sp_addextendedproperty 'MS_Description', '審查日記', 'user', 'dbo', 'table', 'AuditLogs'
           END  
ELSE
           BEGIN  
            exec sp_updateextendedproperty 'MS_Description', '審查日記', 'user', 'dbo', 'table', 'AuditLogs'
           END

go
IF not exists(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
           BEGIN  
            exec sp_addextendedproperty 'MS_Description', '目錄', 'user', 'dbo', 'table', 'Directory'
           END  
ELSE
           BEGIN  
            exec sp_updateextendedproperty 'MS_Description', '目錄', 'user', 'dbo', 'table', 'Directory'
           END

go
IF not exists(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
           BEGIN  
            exec sp_addextendedproperty 'MS_Description', '檔案綁定更多', 'user', 'dbo', 'table', 'FileBindMores'
           END  
ELSE
           BEGIN  
            exec sp_updateextendedproperty 'MS_Description', '檔案綁定更多', 'user', 'dbo', 'table', 'FileBindMores	'
           END

go
IF not exists(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
           BEGIN  
            exec sp_addextendedproperty 'MS_Description', '檔案綁定', 'user', 'dbo', 'table', 'FileBinds'
           END  
ELSE
           BEGIN  
            exec sp_updateextendedproperty 'MS_Description', '檔案綁定', 'user', 'dbo', 'table', 'FileBinds'
           END

go
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '檔案上傳', 'user', 'dbo', 'table', 'FileUploads'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '檔案上傳', 'user', 'dbo', 'table', 'FileUploads'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', 'HTML內容', 'user', 'dbo', 'table', 'Html_Contents'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', 'HTML內容', 'user', 'dbo', 'table', 'Html_Contents'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '物流設置', 'user', 'dbo', 'table', 'LogisticsSettings'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '物流設置', 'user', 'dbo', 'table', 'LogisticsSettings'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '物流類型付款', 'user', 'dbo', 'table', 'LogisticsType_Payments'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '物流類型付款', 'user', 'dbo', 'table', 'LogisticsType_Payments'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '物流類型', 'user', 'dbo', 'table', 'Logisticstypes'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '物流類型', 'user', 'dbo', 'table', 'Logisticstypes'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '映射用戶和角色', 'user', 'dbo', 'table', 'MappingUserAndRoles'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '映射用戶和角色', 'user', 'dbo', 'table', 'MappingUserAndRoles'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '映射用戶和網站', 'user', 'dbo', 'table', 'MappingUserAndWebsites'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '映射用戶和網站', 'user', 'dbo', 'table', 'MappingUserAndWebsites'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '應設網站關係', 'user', 'dbo', 'table', 'MappingWebsiteRelationship'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '應設網站關係', 'user', 'dbo', 'table', 'MappingWebsiteRelationship'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '跑馬燈', 'user', 'dbo', 'table', 'Marquees'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '跑馬燈', 'user', 'dbo', 'table', 'Marquees'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '對象類型', 'user', 'dbo', 'table', 'ObjectTypes'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '對象類型', 'user', 'dbo', 'table', 'ObjectTypes'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '訂單詳細信息', 'user', 'dbo', 'table', 'Order_Details'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '訂單詳細信息', 'user', 'dbo', 'table', 'Order_Details'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '訂單標題', 'user', 'dbo', 'table', 'Order_Headers'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '訂單標題', 'user', 'dbo', 'table', 'Order_Headers'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '付款方式', 'user', 'dbo', 'table', 'PaymentTypes'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '付款方式', 'user', 'dbo', 'table', 'PaymentTypes'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品日誌', 'user', 'dbo', 'table', 'Prod_Logs'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品日誌', 'user', 'dbo', 'table', 'Prod_Logs'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品價格', 'user', 'dbo', 'table', 'Prod_Prices'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品價格', 'user', 'dbo', 'table', 'Prod_Prices'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品規格類型', 'user', 'dbo', 'table', 'Prod_Spec_Types'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品規格類型', 'user', 'dbo', 'table', 'Prod_Spec_Types'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品規格', 'user', 'dbo', 'table', 'Prod_Specs'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品規格', 'user', 'dbo', 'table', 'Prod_Specs'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品庫存', 'user', 'dbo', 'table', 'Prod_Stocks'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品庫存', 'user', 'dbo', 'table', 'Prod_Stocks'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品技術證書', 'user', 'dbo', 'table', 'Prod_TechCerts'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品技術證書', 'user', 'dbo', 'table', 'Prod_TechCerts'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '產品', 'user', 'dbo', 'table', 'Prods'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '產品', 'user', 'dbo', 'table', 'Prods'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '角色', 'user', 'dbo', 'table', 'Roles'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '角色', 'user', 'dbo', 'table', 'Roles'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '購物車', 'user', 'dbo', 'table', 'ShoppingCarts'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '購物車', 'user', 'dbo', 'table', 'ShoppingCarts'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '標籤關聯公司', 'user', 'dbo', 'table', 'Tag_Associates'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '標籤關聯公司', 'user', 'dbo', 'table', 'Tag_Associates'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '標籤組', 'user', 'dbo', 'table', 'Tag_Groups'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '標籤組', 'user', 'dbo', 'table', 'Tag_Groups'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '標籤標籤組', 'user', 'dbo', 'table', 'Tag_TagGroups'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '標籤標籤組', 'user', 'dbo', 'table', 'Tag_TagGroups'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '標籤', 'user', 'dbo', 'table', 'Tags'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '標籤', 'user', 'dbo', 'table', 'Tags'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '技術證書', 'user', 'dbo', 'table', 'TechnicalCertificates'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '技術證書', 'user', 'dbo', 'table', 'TechnicalCertificates'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))  
BEGIN  
    EXEC sp_addextendedproperty 'MS_Description', '第三方串接使用', 'user', 'dbo', 'table', 'ThirdParties'
END  
ELSE
BEGIN  
    EXEC sp_updateextendedproperty 'MS_Description', '第三方串接使用', 'user', 'dbo', 'table', 'ThirdParties'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))
BEGIN
    EXEC sp_addextendedproperty 'MS_Description', '第三方密要', 'user', 'dbo', 'table', 'ThirdPartyKeypairs'
END
ELSE
BEGIN
    EXEC sp_updateextendedproperty 'MS_Description', '第三方密要', 'user', 'dbo', 'table', 'ThirdPartyKeypairs'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))
BEGIN
    EXEC sp_addextendedproperty 'MS_Description', '令牌', 'user', 'dbo', 'table', 'Tokens'
END
ELSE
BEGIN
    EXEC sp_updateextendedproperty 'MS_Description', '令牌', 'user', 'dbo', 'table', 'Tokens'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))
BEGIN
    EXEC sp_addextendedproperty 'MS_Description', '使用者', 'user', 'dbo', 'table', 'Users'
END
ELSE
BEGIN
    EXEC sp_updateextendedproperty 'MS_Description', '使用者', 'user', 'dbo', 'table', 'Users'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))
BEGIN
    EXEC sp_addextendedproperty 'MS_Description', '網站菜單', 'user', 'dbo', 'table', 'WebMenus'
END
ELSE
BEGIN
    EXEC sp_updateextendedproperty 'MS_Description', '網站菜單', 'user', 'dbo', 'table', 'WebMenus'
END

GO
IF NOT EXISTS(SELECT * FROM ::fn_listextendedproperty (NULL, 'user', 'dbo', 'table', '資料表名稱', NULL, NULL))
BEGIN
    EXEC sp_addextendedproperty 'MS_Description', '網站', 'user', 'dbo', 'table', 'Websites'
END
ELSE
BEGIN
    EXEC sp_updateextendedproperty 'MS_Description', '網站', 'user', 'dbo', 'table', 'Websites'
END


