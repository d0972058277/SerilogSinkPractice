# Serilog Sink 實戰專案

一個全面展示多種 Serilog 接收器和日誌基礎設施整合的示範專案。

## 專案概述

本專案展示如何實作和配置各種 Serilog 接收器，將日誌傳送到不同的目的地，包括 Elasticsearch、Kafka、Grafana/Loki、Seq 和檔案記錄。提供使用 Docker Compose 的完整日誌基礎設施堆疊。

## 功能特色

- **多重 Serilog 接收器**: 自訂 Kafka 接收器、Elasticsearch、Loki、Seq 和檔案接收器
- **完整基礎設施**: ELK Stack、Kafka、Grafana/Loki 和 Seq
- **結構化日誌**: JSON 格式化和業務事件分類
- **增強豐富器**: 機器名稱、環境使用者和程序 ID

## 快速開始

### 系統需求

- .NET 9 SDK
- Docker 和 Docker Compose

### 安裝與執行

1. **還原套件並建置:**
   ```bash
   dotnet restore SerilogSinkDemo
   dotnet build SerilogSinkDemo
   ```

2. **啟動日誌基礎設施:**
   ```bash
   docker-compose up -d
   ```

3. **執行示範應用程式:**
   ```bash
   dotnet run --project SerilogSinkDemo
   ```

## 服務端點

| 服務 | URL | 說明 |
|------|-----|------|
| Elasticsearch | http://localhost:9200 | 搜尋和分析引擎 |
| Kibana | http://localhost:5601 | 資料視覺化儀表板 |
| Kafka UI | http://localhost:8081 | Kafka 叢集管理 |
| Grafana | http://localhost:3000 | 監控和可觀測性 |
| Loki | http://localhost:3100 | 日誌聚合系統 |
| Seq | http://localhost:5341 | 結構化日誌平台 |

## 專案結構

```
├── SerilogSinkDemo/          # 主控台應用程式
│   ├── Program.cs            # 主應用程式進入點
│   └── KafkaSink.cs         # 自訂 Kafka 接收器實作
├── logstash/                # Logstash 管線配置
├── logs/                    # 產生的日誌檔案 (git 忽略)
├── docker-compose.yml       # 基礎設施堆疊定義
└── kibana.yml              # Kibana 配置
```

## 日誌接收器

### 1. ELK Stack
全文搜尋和分析，透過 Elasticsearch 儲存，Kibana 視覺化。

### 2. Kafka
即時日誌串流，支援高吞吐量的訊息處理。

### 3. Grafana/Loki
日誌聚合和視覺化，提供強大的查詢和監控功能。

### 4. Seq
具有 Web UI 的結構化日誌平台，支援進階搜尋和過濾。

### 5. File Sink
每日輪替的本地檔案日誌，適合長期儲存和備份。

## 開發指南

### 停止基礎設施
```bash
docker-compose down -v
```

## 授權

本專案僅供示範用途。