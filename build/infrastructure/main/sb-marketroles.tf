# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
module "sb_marketroles" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-namespace?ref=6.0.0"

  name                            = "marketroles"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  private_endpoint_subnet_id      = module.private_endpoints_subnet.id
  private_dns_resource_group_name = data.azurerm_key_vault_secret.pdns_resouce_group_name

  auth_rules            = [
    {
      name    = "listen",
      listen  = true
    },
    {
      name    = "send",
      send    = true
    },
  ]

  tags                  = azurerm_resource_group.this.tags
}

module "sbq_marketroles" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=6.0.0"

  name                = "marketroles"
  namespace_id        = module.sb_marketroles.id
}