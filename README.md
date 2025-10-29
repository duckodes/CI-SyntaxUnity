# Unity 本地端 Push 代碼樣式檢查
### 說明: 自動化設定 Push 檢查

### 專案設定
1. 放入專案至此專案根目錄
2. 開啟 unity，放入 ```../Editor/GitHookInstaller```
3. recompile 後會出現 GitHookInstaller: 已更新並安裝完成
4. 設置完成
5. push 後即會自動檢查語法

### 提交 ```.githook/pre-push```
- 專案成員下次拉取後，將自動再次更新 push 設置