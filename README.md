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

### .editorconfig 中間底線無法判斷
1. 使用自訂分析器 AnalyzerCode
2. bin 資料夾的 AnalyzerCode.dll 放進 Visual Studio 分析器，或開啟 AnalyzerCode.sln 修改並建置專案
3. 開啟 Visual Studio 開啟方案總管(Solution Explorer)>右鍵當前方案或專案>加入>並找到分析器(Analyzer)
4. 選擇 AnalyzerCode.dll 按下確認完成設置