// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

package _self.buildTypes

import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.powerShell

object PublishToAzureArtifacts : BuildType({
    name = "Publish Ed-Fi Learning Standards Core nuget package"

    enablePersonalBuilds = false
    type = BuildTypeSettings.Type.DEPLOYMENT
    maxRunningBuilds = 1

    vcs {
        root(DslContext.settingsRoot)
    }

    params {
        param("testfeed", "https://pkgs.dev.azure.com/%azureArtifacts.organization%/_packaging/EdFiTest/nuget/v3/index.json")
        param("env.VSS_NUGET_EXTERNAL_FEED_ENDPOINTS", """{"endpointCredentials": [{"endpoint": "%testfeed%","username": "%azureArtifacts.edFiBuildAgent.userName%","password": "%azureArtifacts.edFiBuildAgent.accessToken%"}]}""")
    }

    steps {
        powerShell {
            name = "Publish to Azure Artifacts"
            scriptMode = script {
                content = "nuget push -source %testfeed% -apikey az *.nupkg"
            }
        }
    }

    dependencies {
        artifacts(AbsoluteId("Experimental_SMinocha_LearningStandardsClient_BuildLearningStandardsClientCli")) {
            buildRule = lastSuccessful()
            artifactRules = """
                +:**/*.nupkg => .
                -:**/*-pre*.nupkg
            """.trimIndent()
        }
    }
})
