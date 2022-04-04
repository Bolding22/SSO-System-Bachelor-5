job "ajoursso" {
    datacenters = ["dc1"]

    group "db" {
        network {
            mode = "bridge"
        }

        service {
            name = "AjourSPdb"
            port = "3306"

            connect {
                sidecar_service {}
            }
        }

        task "mariadb" {
            driver = "docker"
            config {
                image = "mariadb"
            }

            env {
                MARIADB_ROOT_PASSWORD = "example"
            }

            resources {
                cpu = 2000
                memery = 2000
            }
        }
    }
}