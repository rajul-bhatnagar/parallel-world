import 'package:parallel_world_app/features/catch_up/domain/catch_up_models.dart';

abstract interface class CatchUpGateway {
  Future<CatchUpResult> process({required String worldId});
  Future<CatchUpSummary?> latest({required String worldId});
}
