// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

package _self.buildTypes

import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.powerShell

object PublishToAzureBlob : BuildType({
    name = "Publish Ed-Fi Admin Learning Standards CLI zip"

    vcs {
        root(DslContext.settingsRoot)
    }

    params {
        param("PackageVersion", "%learningStandardsClient.version.major%.%learningStandardsClient.version.minor%.%build.counter%")
    }

    steps {
        powerShell {
            name = "Publish Learning Standards CLI zip to Azure Blob"
            formatStderrAsError = true
            scriptMode = script {
                content = """
                    ${'$'}learningStandardsClientZip = Resolve-Path "EdFi.Admin.LearningStandards.CLI.win-x64.zip"
                    ${'$'}versionSuffix = "%PackageVersion%"
                    azcopy copy (Resolve-Path ${'$'}learningStandardsClientZip) "https://odsassets.blob.core.windows.net/public/test/EdFi.Admin.LearningStandards.CLI.win-${'$'}versionSuffix.zip"
                """.trimIndent()
            }
        }
    }

    failureConditions {
        errorMessage = true
    }

    dependencies {
        artifacts(AbsoluteId("Experimental_SMinocha_LearningStandardsClient_BuildLearningStandardsClientCli")) {
            buildRule = lastSuccessful()
            artifactRules = """
                +:**/*.zip => .
            """.trimIndent()
        }
    }
})
