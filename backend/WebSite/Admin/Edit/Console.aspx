<%@ Page Title="" Language="VB" MasterPageFile="~/Admin/MasterFirst.master" AutoEventWireup="false"
    CodeFile="Console.aspx.vb" Inherits="Console" Trace="false" %>


<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <style>
        body {
            background: #f6f8fb;
        }

        .badge-signal-green {
            background: #2ecc71;
        }

        .badge-signal-yellow {
            background: #f1c40f;
        }

        .badge-signal-red {
            background: #e74c3c;
        }

        .card-metric .value {
            font-size: 2rem;
            font-weight: 700;
            line-height: 1;
        }

        .mono {
            font-family: ui-monospace,SFMono-Regular,Menlo,Monaco,Consolas,"Liberation Mono","Courier New",monospace;
        }

        .card-body {
            flex: 1 1 auto;
            padding: var(--bs-card-spacer-y) var(--bs-card-spacer-x);
            color: var(--bs-card-color);
        }

        .card {
            border: 1px solid #e1e5eb;
            border-radius: 0.6rem;
            background: #fff;
        }

        .card-body {
            flex: 1 1 auto;
            padding: 20px;
            color: var(--bs-card-color);
        }
    </style>
    <asp:UpdatePanel ID="UpdatePanelTelemetry" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Timer ID="TimerTelemetry" runat="server" Interval="5000" OnTick="TimerTelemetry_Tick" />
            <div class="container my-4">

                <!-- Metriche globali -->
                <div class="row g-3">
                    <div class="col-12 col-md-3">
                        <div class="card card-metric shadow-sm">
                            <div class="card-body">
                                <div class="text-secondary">Global Margin</div>
                                <div id="globalMargin" class="value mono">
                                    <asp:Label runat="server" ID="Lbl_GlobalMargin"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 col-md-3">
                        <div class="card card-metric shadow-sm">
                            <div class="card-body">
                                <div class="text-secondary">Global Heavy</div>
                                <div id="globalHeavy" class="value mono">
                                    <asp:Label runat="server" ID="Lbl_globalHeavy"></asp:Label></div>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 col-md-3">
                        <div class="card card-metric shadow-sm">
                            <div class="card-body">
                                <div class="text-secondary">Global Signal</div>
                                <div class="value">
                                    <span id="globalSignal" class="badge">
                                        <asp:Label runat="server" ID="Lbl_globalSignal"></asp:Label></span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 col-md-3">
                        <div class="card card-metric shadow-sm">
                            <div class="card-body">
                                <div class="text-secondary">Tavoli Attivi</div>
                                <div id="tablesActive" class="value mono">
                                    <asp:Label runat="server" ID="Lbl_tablesActive"></asp:Label></div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Controlli -->
                <div class="card mt-4 shadow-sm">
                    <div class="card-body">
                        <div class="d-flex flex-wrap gap-2">
                            <asp:Button ID="btnLoadFactory" class="btn btn-outline-secondary" runat="server" Text="Load Factory" />
                            <asp:Button ID="btnApply" class="btn btn-primary" runat="server" Text="Apply Settings" />
                        </div>
                    </div>
                </div>
                <asp:Label runat="server" ID="lblStatus"></asp:Label>
                <!-- Parametri (editabili) -->
                <div class="card mt-3 shadow-sm">
                    <div class="card-header">Parametri Operativi (Input &rarr; Engine)</div>
                    <div class="card-body">
                        <div id="settingsForm" class="row g-3">
                            <div class="col-6 col-md-2">
                                <label class="form-label">Hmax High</label>1 
                        <asp:TextBox runat="server" CssClass="form-control" ID="TxtHmaxHigh" value="4"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-2">
                                <label class="form-label">Hmax Mid</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="TxtHmaxMid" value="2"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-2">
                                <label class="form-label">Hmax Low</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="TxtHmaxLow" value="0"></asp:TextBox>
                            </div>

                            <div class="col-6 col-md-2">
                                <label class="form-label">Cooldown High</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="TxtCooldownHigh" value="1"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-2">
                                <label class="form-label">Cooldown Mid</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="TxtCooldownMid" value="1"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-2">
                                <label class="form-label">Cooldown Low</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="TxtCooldownLow" value="2"></asp:TextBox>
                            </div>

                            <div class="col-6 col-md-3">
                                <label class="form-label">High Thresh</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="TxtHighThresh" value="500"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-3">
                                <label class="form-label">Low Thresh</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="txtLowThresh" value="-2000"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-3">
                                <label class="form-label">Global Heavy Cap</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="txtGlobalHeavyCap" value="8"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-3">
                                <label class="form-label">PerTable Heavy Limit</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="txtPerTableHeavyLimit" value="3"></asp:TextBox>
                            </div>

                            <div class="col-6 col-md-3">
                                <label class="form-label">Window W10</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="txtWindowW10" value="10"></asp:TextBox>
                            </div>
                            <div class="col-6 col-md-3">
                                <label class="form-label">MaxRun P Allowed</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="txtMaxRunPAllowed" value="3"></asp:TextBox>
                            </div>
                             <div class="col-6 col-md-3">
                                 <label class="form-label">Debt Trigger Ratio</label>
                                 <asp:TextBox runat="server" CssClass="form-control" ID="TxtDebtTriggerRatio" value="0.60"></asp:TextBox>
                             </div>
                            <div class="col-6 col-md-3">
                                <label class="form-label">Sync Delay (ms)</label>
                                <asp:TextBox runat="server" CssClass="form-control" ID="txtSyncDelayMs" value="100"></asp:TextBox>
                            </div>
                            <div class="clearfix"></div>
                            <div class="col-12">
                                <label class="form-label">Levels (array)</label>
                                <asp:TextBox runat="server" Enabled="false" CssClass="form-control" ID="txtLevels" value="[1,3,7,15,35,75,155,340]"></asp:TextBox>
                                <div class="form-text">Formato JSON</div>
                            </div>
                            <div class="col-12">
                                <label class="form-label">HotZones (array di coppie)</label>
                                <asp:TextBox runat="server" Enabled="false" CssClass="form-control" ID="txtHotZones" value="[[11,20],[41,50],[51,60],[61,70]]"></asp:TextBox>
                                <div class="form-text">Formato JSON</div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Telemetria tavoli -->
                <div class="card mt-3 shadow-sm">
                    <div class="card-header">Telemetria Tavoli</div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <telerik:RadGrid Height="550" Font-Size="12PX" ID="RadGrid1" AllowPaging="True" runat="server" OnNeedDataSource="RadGrid1_NeedDataSource"
                                AllowFilteringByColumn="true" EnableHeaderContextMenu="true" AllowSorting="True"
                                PageSize="20" ShowFooter="True" AutoGenerateColumns="true" ShowStatusBar="true">
                                <ClientSettings>
                                    <Scrolling
                                        ScrollHeight="550px" />
                                </ClientSettings>
                                <MasterTableView Font-Size="14">
                                     
                                </MasterTableView>
                                <GroupingSettings CaseSensitive="false" />
                                <ClientSettings>
                                    <Scrolling AllowScroll="true" />
                                    <Selecting AllowRowSelect="false"></Selecting>
                                </ClientSettings>
                                <PagerStyle Mode="NextPrevAndNumeric"></PagerStyle>
                                <FilterMenu EnableTheming="True">
                                    <CollapseAnimation Duration="200" Type="OutQuint" />
                                </FilterMenu>
                            </telerik:RadGrid>
                        </div>
                    </div>
                </div>

            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>

</asp:Content>
